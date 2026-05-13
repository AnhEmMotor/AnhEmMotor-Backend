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
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Features.Outputs.Queries.GetOutputsList;
using Application.Features.Outputs.Queries.GetOutputStatusList;
using Asp.Versioning;
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
public class SalesOrdersController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách đơn hàng c?a khách hàng hiện tại (dựa trên JWT token).
    /// </summary>
    [HttpGet("my-purchases")]
    [ProducesResponseType(typeof(PagedResult<MyOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPurchasesAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var buyerId))
        {
            return Unauthorized(
                new ErrorResponse
                {
                    Errors =
                        [new ErrorDetail
                            {
                                Field = "Authorization",
                                Message = "Kh�ng th? l?y thông tin người dùng t? token."
                            }]
                });
        }
        var query = new GetOutputsByUserIdQuery() { BuyerId = buyerId, SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng c?a id kh�ch h�ng (ch? cho ph�p v�o khi c� quyền xem đơn hàng).
    /// </summary>
    [HttpGet("get-purchases/{id:Guid}")]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<MyOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchasesByIDAsync(
        [FromQuery] SieveModel sieveModel,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsByUserIdQuery() { BuyerId = id, SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Outputs.View)]
    [ProducesResponseType(typeof(PagedResult<OutputItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetOutputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng d� b? x�a (có phân trang, lọc, sắp xếp).
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
    /// L?y danh s�ch tr?ng th�i don h�ng.
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
    /// L?y b?n d? t�n hi?n th? n?i b? c?a tr?ng th�i don h�ng (Ti?ng Vi?t).
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
    /// L?y so d? chuy?n d?i tr?ng th�i don h�ng.
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
    /// L?y danh s�ch tr?ng th�i don h�ng b? kh�a kh�ng cho ph�p s?a th�ng tin chi ti?t.
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
    /// L?y danh s�ch c�c m� tr?ng th�i c� th? Hủy đơn hàng tr?c ti?p.
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
    /// L?y th�ng tin chi ti?t c?a don h�ng.
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
    /// Tạo đơn hàng m?i (d�nh cho ngu?i c� quy?n Tạo đơn hàng).
    /// </summary>
    [HttpPost("by-manager")]
    [HasPermission(Outputs.Create)]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputForAdminAsync(
        [FromBody] CreateOutputByManagerCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<CreateOutputByManagerCommand>() with
        {
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Tạo đơn hàng m?i (d�nh cho c�c t�i kho?n d� dang nh?p).
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(OrderDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutputAsync(
        [FromBody] CreateOutputCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<CreateOutputCommand>() with
        {
            BuyerId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, SaleOrders.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho ph�p s?a don h�ng do ch�nh m�nh t?o ra)
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
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = new CancelOrderByBuyerCommand
        {
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật đơn hàng (Cho ph�p s?a t?t c? don h�ng, nhung ch? cho ph�p c?p nh?t khi v� ch? khi c� quy?n ch?nh s?a
    /// don h�ng)
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
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<UpdateOutputForManagerCommand>() with
        {
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
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
    /// Khôi phục đơn hàng d� b? x�a.
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
    /// Kh�i ph?c nhi?u don h�ng d� b? x�a c�ng l�c.
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
}

