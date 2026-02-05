using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
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
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsList;
using Asp.Versioning;
using Domain.Constants;
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
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý đơn hàng/phiếu xuất.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý đơn hàng/phiếu xuất")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SalesOrdersController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách đơn hàng của khách hàng hiện tại (dựa trên JWT token).
    /// </summary>
    [HttpGet("my-purchases")]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPurchasesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var buyerId))
        {
            return Unauthorized(
                new ErrorResponse
                {
                    Errors =
                        [ new ErrorDetail
                            {
                                Field = "Authorization",
                                Message = "Không thể lấy thông tin người dùng từ token."
                            } ]
                });
        }

        var query = new Application.Features.Outputs.Queries.GetOutputsByUserId.GetOutputsByUserIdQuery()
        {
            BuyerId = buyerId,
            SieveModel = sieveModel
        };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng của id khách hàng (chỉ cho phép vào khi có quyền xem đơn hàng).
    /// </summary>
    [HttpGet("get-purchases/{id:Guid}")]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchasesByIDAsync(
        [FromQuery] SieveModel sieveModel,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new Application.Features.Outputs.Queries.GetOutputsByUserId.GetOutputsByUserIdQuery()
        {
            BuyerId = id,
            SieveModel = sieveModel
        };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedOutputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của đơn hàng.
    /// </summary>
    [HttpGet("{id:int}", Name = RouteNames.SaleOrders.GetById)]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputForAdminAsync(
        [FromBody] CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateOutputByManagerCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Tạo đơn hàng mới (dành cho các tài khoản đã đăng nhập).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputAsync(
        [FromBody] CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateOutputCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, RouteNames.SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho phép sửa đơn hàng do chính mình tạo ra)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputForManagerAsync(
        int id,
        [FromBody] UpdateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<UpdateOutputCommand>() with
        {
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho phép sửa tất cả đơn hàng, nhưng chỉ cho phép cập nhật khi và chỉ khi có quyền chỉnh sửa
    /// đơn hàng)
    /// </summary>
    [HttpPut("for-manager/{id:int}")]
    [HasPermission(Outputs.Edit)]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputAsync(
        int id,
        [FromBody] UpdateOutputForManagerCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateOutputForManagerCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }


    /// <summary>
    /// Cập nhật trạng thái của đơn hàng.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Outputs.ChangeStatus)]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputStatusAsync(
        int id,
        [FromBody] UpdateOutputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<UpdateOutputStatusCommand>() with 
        { 
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều đơn hàng cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [HasPermission(Outputs.ChangeStatus)]
    [ProducesResponseType(typeof(List<OutputResponse>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(List<OutputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyOutputsAsync(
        [FromBody] RestoreManyOutputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyOutputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
