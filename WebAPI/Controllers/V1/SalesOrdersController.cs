using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.Outputs.Commands.CancelOrderByBuyer;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.RestoreOutput;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Outputs.Queries.GetDeletedOutputsList;
using Application.Features.Outputs.Queries.GetOrderCancellableStatuses;
using Application.Features.Outputs.Queries.GetOrderLockedStatuses;
using Application.Features.Outputs.Queries.GetOrderStatusMap;
using Application.Features.Outputs.Queries.GetOrderStatusTransitionMap;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputForCurrentUserById;
using Application.Features.Outputs.Queries.GetOutputsByUserIdForManager;
using Application.Features.Outputs.Queries.GetOutputsForCurrentUser;
using Application.Features.Outputs.Queries.GetOutputsList;
using Application.Features.Outputs.Queries.GetOutputStatusList;
using Application.Features.Outputs.Queries.GetVehicleAssignmentRequirements;
using Application.Features.Outputs.Queries.GetVehicleAssignmentStatuses;
using Application.Interfaces.Services;
using Asp.Versioning;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Constants.Permission.Permissions;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý đơn hàng/phiếu xuất.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý đơn hàng/phiếu xuất")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SalesOrdersController(
    IMediator mediator,
    ICurrentUserContext currentUserContext) : ApiController
{
    /// <summary>
    /// Lấy danh sách đơn hàng của người dùng hiện tại (dựa trên JWT token).
    /// </summary>
    [HttpGet("my-purchases")]
    [ProducesResponseType(typeof(PagedResult<MyOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPurchasesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsForCurrentUserQuery(sieveModel);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy chi tiết một đơn hàng thuộc về người dùng hiện tại.
    /// </summary>
    [HttpGet("my-purchases/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPurchaseByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOutputForCurrentUserByIdQuery(id), cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng theo ID khách hàng (dành cho manager có quyền Outputs.View).
    /// </summary>
    [HttpGet("get-purchases/{id:Guid}")]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchasesByIDAsync(
        [FromQuery] SieveModel sieveModel,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsByUserIdForManagerQuery(id, sieveModel);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách phiếu bán hàng đã xác nhận.
    /// </summary>
    [HttpGet("confirmed")]
    [HasPermission(Outputs.ViewConfirmed)]
    [ProducesResponseType(typeof(PagedResult<OutputItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfirmedOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        return await GetOutputsByStatusesAsync(
            sieveModel,
            OrderStatus.ConfirmedOrderStatuses,
            cancellationToken)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Lấy danh sách phiếu tạm chưa xác nhận.
    /// </summary>
    [HttpGet("unconfirmed")]
    [HasPermission(Outputs.ViewUnconfirmed)]
    [ProducesResponseType(typeof(PagedResult<OutputItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnconfirmedOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        return await GetOutputsByStatusesAsync(
            sieveModel,
            OrderStatus.UnconfirmedOrderStatuses,
            cancellationToken)
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedOutputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách trạng thái đơn hàng.
    /// </summary>
    [HttpGet("status")]
    [RequiresAnyPermissions(Outputs.View, Outputs.Create, Outputs.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetOutputStatusListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy bản đồ tên hiển thị nội bộ của trạng thái đơn hàng (Tiếng Việt).
    /// </summary>
    [HttpGet("status-map")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusMapAsync(CancellationToken cancellationToken)
    {
        var query = new GetOrderStatusMapQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy sơ đồ chuyển đổi trạng thái đơn hàng.
    /// </summary>
    [HttpGet("transition-map")]
    [RequiresAnyPermissions(Outputs.Create, Outputs.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransitionMapAsync(CancellationToken cancellationToken)
    {
        var query = new GetOrderStatusTransitionMapQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách trạng thái đơn hàng bị khóa không cho phép sửa thông tin chi tiết.
    /// </summary>
    [HttpGet("locked-statuses")]
    [Authorize]
    [ProducesResponseType(typeof(OrderLockStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLockedStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetOrderLockedStatusesQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các mã trạng thái có thể hủy đơn hàng trực tiếp.
    /// </summary>
    [HttpGet("cancellable-statuses")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCancellableStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetOrderCancellableStatusesQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách các mã trạng thái yêu cầu gán xe (VIN).
    /// </summary>
    [HttpGet("vehicle-assignment-statuses")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleAssignmentStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetVehicleAssignmentStatusesQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy yêu cầu chọn VIN theo từng dòng sản phẩm trước khi đổi trạng thái đơn hàng.
    /// </summary>
    [HttpGet("{id:int}/vehicle-assignment-requirements")]
    [RequiresAnyPermissions(Outputs.View, Outputs.ChangeStatus)]
    [ProducesResponseType(typeof(VehicleAssignmentRequirementResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleAssignmentRequirementsAsync(
        int id,
        [FromQuery] string targetStatusId,
        CancellationToken cancellationToken)
    {
        var query = new GetVehicleAssignmentRequirementsQuery { Id = id, TargetStatusId = targetStatusId };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của đơn hàng.
    /// </summary>
    [HttpGet("{id:int}", Name = SaleOrders.GetById)]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOutputByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetOutputByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo đơn hàng mới (dành cho người có quyền tạo đơn hàng).
    /// </summary>
    [HttpPost("by-manager")]
    [HasPermission(Outputs.Create)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputForAdminAsync(
        [FromBody] CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetUserId();
        var command = request.Adapt<CreateOutputByManagerCommand>() with
        {
            CurrentUserId = currentUserId
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Tạo đơn hàng mới (dành cho các tài khoản đã đăng nhập).
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputAsync(
        [FromBody] CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetUserId();
        var command = request.Adapt<CreateOutputCommand>() with
        {
            BuyerId = currentUserId
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho phép sửa đơn hàng do chính mình tạo ra).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputForManagerAsync(
        int id,
        [FromBody] UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetUserId();
        var command = request.Adapt<UpdateOutputCommand>() with
        {
            Id = id,
            CurrentUserId = currentUserId
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Hủy đơn hàng (Dành cho người sở hữu đơn hàng).
    /// </summary>
    [HttpPatch("{id:int}/cancel-my-order")]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelMyOrderAsync(int id, CancellationToken cancellationToken)
    {
        var command = new CancelOrderByBuyerCommand
        {
            Id = id,
            CurrentUserId = currentUserContext.GetUserId()
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho phép sửa tất cả đơn hàng, nhưng chỉ cho phép cập nhật khi và chỉ khi có quyền chỉnh sửa
    /// đơn hàng).
    /// </summary>
    [HttpPut("for-manager/{id:int}")]
    [HasPermission(Outputs.Edit)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputAsync(
        int id,
        [FromBody] UpdateOutputForManagerCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.GetUserId();
        var command = request.Adapt<UpdateOutputForManagerCommand>() with
        {
            Id = id,
            CurrentUserId = currentUserId
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của đơn hàng.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Outputs.ChangeStatus)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputStatusAsync(
        int id,
        [FromBody] UpdateOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateOutputStatusCommand>() with
        {
            Id = id,
            CurrentUserId = currentUserContext.GetUserId()
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều đơn hàng cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [HasPermission(Outputs.ChangeStatus)]
    [ProducesResponseType(typeof(List<OutputItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyOutputStatusAsync(
        [FromBody] UpdateManyOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyOutputStatusCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa đơn hàng.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOutputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteOutputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều đơn hàng cùng lúc.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyOutputsAsync(
        [FromBody] DeleteManyOutputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyOutputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục đơn hàng đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreOutputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreOutputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều đơn hàng đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [ProducesResponseType(typeof(List<OutputItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyOutputsAsync(
        [FromBody] RestoreManyOutputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyOutputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    private async Task<IActionResult> GetOutputsByStatusesAsync(
        SieveModel sieveModel,
        IReadOnlyCollection<string> statusIds,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsListQuery { SieveModel = sieveModel, StatusIds = statusIds };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
