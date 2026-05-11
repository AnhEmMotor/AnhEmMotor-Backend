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
using Application.Features.Inputs.Queries.GetInputStatusList;
using Asp.Versioning;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WebAPI.Controllers.Base;
using Domain.Constants.Permission.Permissions;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Qu?n lý phi?u nh?p hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý phi?u nh?p hàng")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReceiptsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y danh sách phi?u nh?p (có phân trang, l?c, s?p x?p).
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
    /// L?y danh sách tr?ng thái phi?u nh?p.
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
    /// L?y danh sách phi?u nh?p dã b? xóa (có phân trang, l?c, s?p x?p).
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
    /// L?y thông tin chi ti?t c?a phi?u nh?p.
    /// </summary>
    [HttpGet("{id:int}", Name = Domain.Constants.RouteNames.InventoryReceipts.GetById)]
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
    /// L?y danh sách phi?u nh?p theo nhà cung c?p.
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
        return HandleCreated(
            result,
            Domain.Constants.RouteNames.InventoryReceipts.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : null });
    }

    /// <summary>
    /// Clone phi?u nh?p t? phi?u nh?p g?c. Ch? clone các s?n ph?m còn h?p l? (chua xoá, còn dang bán).
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
        return HandleCreated(
            result,
            Domain.Constants.RouteNames.InventoryReceipts.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : null });
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
    /// C?p nh?t tr?ng thái c?a phi?u nh?p.
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
    /// C?p nh?t ghi chú c?a phi?u nh?p.
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
    /// C?p nh?t tr?ng thái c?a nhi?u phi?u nh?p cùng lúc.
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
    /// Xóa phi?u nh?p.
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
    /// Xóa nhi?u phi?u nh?p cùng lúc.
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
    /// Khôi ph?c phi?u nh?p dã b? xóa.
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
    /// Khôi ph?c nhi?u phi?u nh?p dã b? xóa cùng lúc.
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


