using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Features.Inputs.Commands.CloneInput;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreInput;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputNotes;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Application.Features.Inputs.Queries.GetDeletedInputsList;
using Application.Features.Inputs.Queries.GetInputById;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Inputs.Queries.GetInputsList;
using Application.Features.Inputs.Queries.GetInventoryReceiptStats;
using Application.Features.Inputs.Queries.GetInputStatusList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Constants.RouteNames;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Qu?n l� phi?u nh?p h�ng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n l� phi?u nh?p h�ng")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReceiptsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y danh s�ch phi?u nh?p (c� ph�n trang, l?c, s?p x?p).
    /// </summary>
    [HttpGet]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(PagedResult<InputListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê cho phần phiếu nhập kho.
    /// </summary>
    [HttpGet("statistics")]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(InventoryReceiptStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReceiptStatsAsync(CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptStatsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch tr?ng th�i phi?u nh?p.
    /// </summary>
    [HttpGet("status")]
    [RequiresAnyPermissions(Inputs.View, Inputs.Create, Inputs.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetInputStatusListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch phi?u nh?p d� b? x�a (c� ph�n trang, l?c, s?p x?p).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(PagedResult<InputListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedInputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedInputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y th�ng tin chi ti?t c?a phi?u nh?p.
    /// </summary>
    [HttpGet("{id:int}", Name = InventoryReceipts.GetById)]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInputByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetInputByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// L?y danh s�ch phi?u nh?p theo nh� cung c?p.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [RequiresAllPermissions(Suppliers.View, Inputs.View)]
    [ProducesResponseType(typeof(PagedResult<InputListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputsBySupplierIdAsync(
        int supplierId,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsBySupplierIdQuery() { SieveModel = sieveModel, SupplierId = supplierId };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// T?o phi?u nh?p m?i.
    /// </summary>
    [HttpPost]
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInputAsync(
        [FromBody] CreateInputCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInputCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, InventoryReceipts.GetById, new { id = result.IsSuccess ? result.Value?.Id : null });
    }

    /// <summary>
    /// Clone phi?u nh?p t? phi?u nh?p g?c. Ch? clone c�c s?n ph?m c�n h?p l? (chua xo�, c�n dang b�n).
    /// </summary>
    [HttpPost("{id:int}/clone")]
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloneInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new CloneInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, InventoryReceipts.GetById, new { id = result.IsSuccess ? result.Value?.Id : null });
    }

    /// <summary>
    /// C?p nh?t phi?u nh?p.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Inputs.Edit)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInputAsync(
        int id,
        [FromBody] UpdateInputCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<UpdateInputCommand>() with 
        { 
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t tr?ng th�i c?a phi?u nh?p.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInputStatusAsync(
        int id,
        [FromBody] UpdateInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var command = request.Adapt<UpdateInputStatusCommand>() with 
        { 
            Id = id,
            CurrentUserId = Guid.TryParse(currentUserId, out var guid) ? guid : null
        };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t ghi ch� c?a phi?u nh?p.
    /// </summary>
    [HttpPatch("{id:int}/notes")]
    [HasPermission(Inputs.Edit)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInputNotesAsync(
        int id,
        [FromBody] UpdateInputNotesCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// C?p nh?t tr?ng th�i c?a nhi?u phi?u nh?p c�ng l�c.
    /// </summary>
    [HttpPatch("status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(List<InputDetailResponse>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyInputStatusAsync(
        [FromBody] UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyInputStatusCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// X�a phi?u nh?p.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// X�a nhi?u phi?u nh?p c�ng l�c.
    /// </summary>
    [HttpDelete]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyInputsAsync(
        [FromBody] DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Kh�i ph?c phi?u nh?p d� b? x�a.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(InputDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Kh�i ph?c nhi?u phi?u nh?p d� b? x�a c�ng l�c.
    /// </summary>
    [HttpPost("restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(List<InputDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyInputsAsync(
        [FromBody] RestoreManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}

