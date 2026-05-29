using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.DeleteInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.DeleteManyInventoryReceipts;
using Application.Features.InventoryReceipts.Commands.RestoreInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.RestoreManyInventoryReceipts;
using Application.Features.InventoryReceipts.Commands.SendInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptNotes;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;
using Application.Features.InventoryReceipts.Queries.GetDeletedInventoryReceiptsList;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsList;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStats;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStatusList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý phiếu nhập hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý phiếu nhập hàng")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReceiptsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách phiếu nhập (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet]
    [HasPermission(InventoryReceipts.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryReceiptListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReceiptsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê cho phần phiếu nhập kho.
    /// </summary>
    [HttpGet("statistics")]
    [HasPermission(InventoryReceipts.View)]
    [ProducesResponseType(typeof(InventoryReceiptStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReceiptStatsAsync(CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptStatsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách trạng thái phiếu nhập.
    /// </summary>
    [HttpGet("status")]
    [RequiresAnyPermissions(
        InventoryReceipts.View,
        InventoryReceipts.Create,
        InventoryReceipts.Edit)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReceiptStatusesAsync(CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptStatusListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    [HttpGet("deleted")]
    [HasPermission(InventoryReceipts.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryReceiptListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedInventoryReceiptsAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedInventoryReceiptsListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của phiếu nhập.
    /// </summary>
    [HttpGet("{id:int}", Name = Domain.Constants.RouteNames.InventoryReceipts.GetById)]
    [HasPermission(InventoryReceipts.View)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventoryReceiptByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách phiếu nhập theo nhà cung cấp.
    /// </summary>
    [HttpGet("by-supplier/{supplierId:int}")]
    [RequiresAllPermissions(Suppliers.View, InventoryReceipts.View)]
    [ProducesResponseType(typeof(PagedResult<InventoryReceiptListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReceiptsBySupplierIdAsync(
        int supplierId,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryReceiptsBySupplierIdQuery() { SieveModel = sieveModel, SupplierId = supplierId };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo phiếu nhập mới.
    /// </summary>
    [HttpPost]
    [HasPermission(InventoryReceipts.Create)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInventoryReceiptAsync(
        [FromBody] CreateInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateInventoryReceiptCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(
            result,
            Domain.Constants.RouteNames.InventoryReceipts.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : null });
    }

    /// <summary>
    /// Clone phiếu nhập từ phiếu nhập gốc. Chỉ clone các sản phẩm còn hợp lệ (chưa xóa, còn đang bán).
    /// </summary>
    [HttpPost("{id:int}/clone")]
    [HasPermission(InventoryReceipts.Create)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloneInventoryReceiptAsync(int id, CancellationToken cancellationToken)
    {
        var command = new CloneInventoryReceiptCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(
            result,
            Domain.Constants.RouteNames.InventoryReceipts.GetById,
            new { id = result.IsSuccess ? result.Value?.Id : null });
    }

    /// <summary>
    /// Cập nhật phiếu nhập.
    /// </summary>
    [HttpPut("{id:int}")]
    [RequiresAnyPermissions(
        InventoryReceipts.Edit,
        InventoryReceipts.ApproveReject)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInventoryReceiptAsync(
        int id,
        [FromBody] UpdateInventoryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInventoryReceiptCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của phiếu nhập (Approve hoặc Reject)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [HasPermission(InventoryReceipts.ApproveReject)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInventoryReceiptStatusAsync(
        int id,
        [FromBody] UpdateInventoryReceiptStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateInventoryReceiptStatusCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Gửi phiếu nhập hàng (chuyển trạng thái từ nháp sang đã gửi).
    /// </summary>
    [HttpPost("{id:int}/send")]
    [HasPermission(InventoryReceipts.Send)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendInventoryReceiptAsync(int id, CancellationToken cancellationToken)
    {
        var command = new SendInventoryReceiptCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật ghi chú của phiếu nhập.
    /// </summary>
    [HttpPatch("{id:int}/notes")]
    [HasPermission(InventoryReceipts.Edit)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventoryReceiptNotesAsync(
        int id,
        [FromBody] UpdateInventoryReceiptNotesCommand request,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa phiếu nhập.
    /// </summary>
    [HttpDelete("{id:int}")]
    [RequiresAnyPermissions(
        InventoryReceipts.Delete,
        InventoryReceipts.ApproveReject)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInventoryReceiptAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInventoryReceiptCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều phiếu nhập cùng lúc.
    /// </summary>
    [HttpDelete]
    [RequiresAnyPermissions(
        InventoryReceipts.Delete,
        InventoryReceipts.ApproveReject)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManyInventoryReceiptsAsync(
        [FromBody] DeleteManyInventoryReceiptsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManyInventoryReceiptsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục phiếu nhập đã bị xóa.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [HasPermission(InventoryReceipts.Delete)]
    [ProducesResponseType(typeof(InventoryReceiptDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreInventoryReceiptAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreInventoryReceiptCommand() { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều phiếu nhập đã bị xóa cùng lúc.
    /// </summary>
    [HttpPost("restore")]
    [HasPermission(InventoryReceipts.Delete)]
    [ProducesResponseType(typeof(List<InventoryReceiptDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreManyInventoryReceiptsAsync(
        [FromBody] RestoreManyInventoryReceiptsCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManyInventoryReceiptsCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}