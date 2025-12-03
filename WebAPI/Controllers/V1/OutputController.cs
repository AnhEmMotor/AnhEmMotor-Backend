using Application.ApiContracts.Output;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.RestoreOutput;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Outputs.Queries.GetDeletedOutputsList;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsList;
using Asp.Versioning;
using Domain.Helpers;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý đơn hàng/phiếu xuất.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class OutputController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách đơn hàng (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputs([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetOutputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách đơn hàng đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(PagedResult<OutputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedOutputs(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedOutputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của đơn hàng.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOutputById(int id, CancellationToken cancellationToken)
    {
        var query = new GetOutputByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Tạo đơn hàng mới.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOutput(
        [FromBody] CreateOutputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateOutputCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return CreatedAtAction(nameof(GetOutputById), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Cập nhật đơn hàng.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutput(
        int id,
        [FromBody] UpdateOutputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateOutputCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của đơn hàng.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOutputStatus(
        int id,
        [FromBody] UpdateOutputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateOutputStatusCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều đơn hàng cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [ProducesResponseType(typeof(List<OutputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyOutputStatus(
        [FromBody] UpdateManyOutputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyOutputStatusCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Xóa đơn hàng.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOutput(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteOutputCommand(id);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Xóa nhiều đơn hàng cùng lúc.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyOutputs(
        [FromBody] DeleteManyOutputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyOutputsCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Khôi phục đơn hàng đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(OutputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreOutput(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreOutputCommand(id);
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều đơn hàng đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [ProducesResponseType(typeof(List<OutputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyOutputs(
        [FromBody] RestoreManyOutputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyOutputsCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }
}
