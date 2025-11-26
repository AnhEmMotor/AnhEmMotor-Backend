using Application.ApiContracts.Input;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreInput;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Application.Features.Inputs.Queries.GetDeletedInputsList;
using Application.Features.Inputs.Queries.GetInputById;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Inputs.Queries.GetInputsList;
using Asp.Versioning;
using Domain.Helpers;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý phiếu nhập hàng.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InputController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách phiếu nhập (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputs(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedInputs(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedInputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của phiếu nhập.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInputById(int id, CancellationToken cancellationToken)
    {
        var query = new GetInputByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if (error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [ProducesResponseType(typeof(PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputsBySupplierId(
        int supplierId,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInputsBySupplierIdQuery(supplierId, sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Tạo phiếu nhập mới.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInput(
        [FromBody] CreateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInputCommand>();
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return CreatedAtAction(nameof(GetInputById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Cập nhật phiếu nhập.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInput(
        int id,
        [FromBody] UpdateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputCommand>() with { Id = id };
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật trạng thái của phiếu nhập.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInputStatus(
        int id,
        [FromBody] UpdateInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputStatusCommand>() with { Id = id };
        var response = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateManyInputStatus(
        [FromBody] UpdateManyInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyInputStatusCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Xóa phiếu nhập.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInput(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInputCommand(id);
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Xóa nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteManyInputs(
        [FromBody] DeleteManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInputsCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Khôi phục phiếu nhập đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInput(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInputCommand(id);
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }

    /// <summary>
    /// Khôi phục nhiều phiếu nhập đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RestoreManyInputs(
        [FromBody] RestoreManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInputsCommand>();
        await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return NoContent();
    }
}
