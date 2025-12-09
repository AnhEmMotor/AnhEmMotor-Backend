using Application.ApiContracts.Input.Responses;
using Application.Features.Inputs.Commands.CloneInput;
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
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý phiếu nhập hàng.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu nhập hàng")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReceiptsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách phiếu nhập (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputs([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetInputsListQuery(sieveModel);
        var pagedResult = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(pagedResult);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
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
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInputById(int id, CancellationToken cancellationToken)
    {
        var query = new GetInputByIdQuery(id);
        var (data, error) = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [RequiresAllPermissions(Suppliers.View, Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
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
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInput(
        [FromBody] Application.ApiContracts.Input.Requests.CreateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInputCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return CreatedAtAction(nameof(GetInputById), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Clone phiếu nhập từ phiếu nhập gốc. Chỉ clone các sản phẩm còn hợp lệ (chưa xoá, còn đang bán).
    /// </summary>
    [HttpPost("{id:int}/clone")]
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloneInput(int id, CancellationToken cancellationToken)
    {
        var command = new CloneInputCommand(id);
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);

        if(error != null)
        {
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetInputById), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Cập nhật phiếu nhập.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Inputs.Edit)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInput(
        int id,
        [FromBody] Application.ApiContracts.Input.Requests.UpdateInputRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của phiếu nhập.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInputStatus(
        int id,
        [FromBody] Application.ApiContracts.Input.Requests.UpdateInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputStatusCommand>() with { Id = id };
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(List<InputResponse>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyInputStatus(
        [FromBody] Application.ApiContracts.Input.Requests.UpdateManyInputStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyInputStatusCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return BadRequest(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Xóa phiếu nhập.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInput(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInputCommand(id);
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Xóa nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpDelete]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyInputs(
        [FromBody] Application.ApiContracts.Input.Requests.DeleteManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInputsCommand>();
        var error = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return NoContent();
    }

    /// <summary>
    /// Khôi phục phiếu nhập đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInput(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInputCommand(id);
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }

    /// <summary>
    /// Khôi phục nhiều phiếu nhập đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(List<InputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyInputs(
        [FromBody] Application.ApiContracts.Input.Requests.RestoreManyInputsRequest request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInputsCommand>();
        var (data, error) = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        if(error != null)
        {
            return NotFound(error);
        }
        return Ok(data);
    }
}
