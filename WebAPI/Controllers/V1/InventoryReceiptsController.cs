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
using System.Security.Claims;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý phiếu nhập hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu nhập hàng")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReceiptsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách phiếu nhập (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInputsAsync([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = new GetInputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedInputsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedInputsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của phiếu nhập.
    /// </summary>
    [HttpGet("{id:int}")]
    [HasPermission(Inputs.View)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInputByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetInputByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [RequiresAllPermissions(Suppliers.View, Inputs.View)]
    [ProducesResponseType(typeof(Domain.Primitives.PagedResult<InputResponse>), StatusCodes.Status200OK)]
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
    /// Tạo phiếu nhập mới.
    /// </summary>
    [HttpPost]
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInputAsync(
        [FromBody] CreateInputCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInputCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Clone phiếu nhập từ phiếu nhập gốc. Chỉ clone các sản phẩm còn hợp lệ (chưa xoá, còn đang bán).
    /// </summary>
    [HttpPost("{id:int}/clone")]
    [HasPermission(Inputs.Create)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloneInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new CloneInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật phiếu nhập.
    /// </summary>
    [HttpPut("{id:int}")]
    [HasPermission(Inputs.Edit)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInputAsync(
        int id,
        [FromBody] UpdateInputCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInputCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của phiếu nhập.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
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
    /// Cập nhật trạng thái của nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpPatch("status")]
    [HasPermission(Inputs.ChangeStatus)]
    [ProducesResponseType(typeof(List<InputResponse>), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManyInputStatusAsync(
        [FromBody] UpdateManyInputStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManyInputStatusCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa phiếu nhập.
    /// </summary>
    [HttpDelete("{id:int}")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpDelete]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyInputsAsync(
        [FromBody] DeleteManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục phiếu nhập đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(InputResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInputAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInputCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều phiếu nhập đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [HasPermission(Inputs.Delete)]
    [ProducesResponseType(typeof(List<InputResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Application.Common.Models.ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyInputsAsync(
        [FromBody] RestoreManyInputsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInputsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
