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
/// Qu?n lý don hàng/phi?u xu?t.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý don hàng/phi?u xu?t")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SalesOrdersController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y danh sách don hàng c?a khách hàng hi?n t?i (d?a trên JWT token).
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
                                Message = "Không th? l?y thông tin ngu?i dùng t? token."
                            }]
                });
        }
        var query = new GetOutputsByUserIdQuery() { BuyerId = buyerId, SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh sách don hàng c?a id khách hàng (ch? cho phép vào khi có quy?n xem don hàng).
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
    /// L?y danh sách don hàng (có phân trang, l?c, s?p x?p).
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
    /// L?y danh sách don hàng dã b? xóa (có phân trang, l?c, s?p x?p).
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
    /// L?y danh sách tr?ng thái don hàng.
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
    /// L?y b?n d? tên hi?n th? n?i b? c?a tr?ng thái don hàng (Ti?ng Vi?t).
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
    /// L?y so d? chuy?n d?i tr?ng thái don hàng.
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
    /// L?y danh sách tr?ng thái don hàng b? khóa không cho phép s?a thông tin chi ti?t.
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
    /// L?y danh sách các mã tr?ng thái có th? h?y don hàng tr?c ti?p.
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
    /// L?y thông tin chi ti?t c?a don hàng.
    /// </summary>
    [HttpGet("{id:int}", Name = RouteNames.SaleOrders.GetById)]
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
    /// T?o don hàng m?i (dành cho ngu?i có quy?n t?o don hàng).
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
        return HandleCreated(
            result,
            RouteNames.SaleOrders.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// T?o don hàng m?i (dành cho các tài kho?n dã dang nh?p).
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
        return HandleCreated(
            result,
            RouteNames.SaleOrders.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// C?p nh?t don hàng (Cho phép s?a don hàng do chính mình t?o ra)
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
    /// H?y don hàng (Dành cho ngu?i s? h?u don hàng).
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
    /// C?p nh?t don hàng (Cho phép s?a t?t c? don hàng, nhung ch? cho phép c?p nh?t khi và ch? khi có quy?n ch?nh s?a
    /// don hàng)
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
    /// C?p nh?t tr?ng thái c?a don hàng.
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
    /// C?p nh?t tr?ng thái c?a nhi?u don hàng cùng lúc.
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
    /// Xóa don hàng.
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
    /// Xóa nhi?u don hàng cùng lúc.
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
    /// Khôi ph?c don hàng dã b? xóa.
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
    /// Khôi ph?c nhi?u don hàng dã b? xóa cùng lúc.
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
