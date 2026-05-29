using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Features.InventoryReceipts.Queries.GetInputsBySupplierId;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.DeleteSupplier;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.RestoreSupplier;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Application.Features.Suppliers.Queries.ExportSuppliers;
using Application.Features.Suppliers.Queries.GetDeletedSuppliersList;
using Application.Features.Suppliers.Queries.GetPartnerTypesList;
using Application.Features.Suppliers.Queries.GetSupplierById;
using Application.Features.Suppliers.Queries.GetSuppliersList;
using Application.Features.Suppliers.Queries.GetSuppliersListForInputManager;
using Application.Features.Suppliers.Queries.GetSupplierStatistics;
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
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý danh sách nhà cung cấp.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý danh sách nhà cung cấp")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SupplierController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách các loại đối tác.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách loại đối tác.</returns>
    [HttpGet("partner-types")]
    [RequiresAnyPermissions(Suppliers.View, Suppliers.Create, Suppliers.Edit)]
    [ProducesResponseType(typeof(List<PartnerTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerTypesAsync(CancellationToken cancellationToken)
    {
        var query = new GetPartnerTypesListQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách nhà cung cấp.</returns>
    [HttpGet]
    [RequiresAnyPermissions(Suppliers.View, Domain.Constants.Permission.Permissions.Quotations.Edit, Domain.Constants.Permission.Permissions.Quotations.Create)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp đã bị xóa (có phân trang, lọc, sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách nhà cung cấp đã xóa.</returns>
    [HttpGet("deleted")]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeletedSuppliersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetDeletedSuppliersListQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thông tin của nhà cung cấp được chọn.
    /// </summary>
    /// <param name="id">Mã nhà cung cấp cần lấy thông tin.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chi tiết nhà cung cấp.</returns>
    [HttpGet("{id:int}", Name = Supplier.GetById)]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierByIdAsync(int id, CancellationToken cancellationToken)
    {
        var query = new GetSupplierByIdQuery() { Id = id };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy lịch sử nhập hàng của nhà cung cấp.
    /// </summary>
    /// <param name="id">ID nhà cung cấp.</param>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Lịch sử nhập hàng.</returns>
    [HttpGet("{id:int}/purchase-history")]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(PagedResult<SupplierPurchaseHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierPurchaseHistoryAsync(
        int id,
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSupplierPurchaseHistoryQuery() { SupplierId = id, SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo nhà cung cấp mới.
    /// </summary>
    /// <param name="request">Thông tin nhà cung cấp cần tạo.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Nhà cung cấp vừa được tạo.</returns>
    [HttpPost]
    [HasPermission(Suppliers.Create)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSupplierAsync(
        [FromBody] CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateSupplierCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleCreated(result, Supplier.GetById, new { id = result.IsSuccess ? result.Value?.Id : 0 });
    }

    /// <summary>
    /// Lấy danh sách nhà cung cấp cho việc nhập hàng (chỉ dành cho người dùng có quyền thêm và sửa phiếu nhập hàng).
    /// </summary>
    /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách nhà cung cấp cho việc nhập hàng.</returns>
    [HttpGet("for-InventoryReceipt")]
    [RequiresAnyPermissions(Domain.Constants.Permission.Permissions.InventoryReceipts.Create, Domain.Constants.Permission.Permissions.InventoryReceipts.Edit)]
    [ProducesResponseType(typeof(PagedResult<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliersForInputAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetSuppliersListForInputManagerQuery() { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật thông tin nhà cung cấp.
    /// </summary>
    /// <param name="id">ID nhà cung cấp cần cập nhật.</param>
    /// <param name="request">Thông tin nhà cung cấp mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin nhà cung cấp sau khi cập nhật.</returns>
    [HttpPut("{id:int}")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierAsync(
        int id,
        [FromBody] UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của nhà cung cấp.
    /// </summary>
    /// <param name="id">ID nhà cung cấp cần cập nhật trạng thái.</param>
    /// <param name="request">Trạng thái mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin nhà cung cấp sau khi cập nhật trạng thái.</returns>
    [HttpPatch("{id:int}/status")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierStatusAsync(
        int id,
        [FromBody] UpdateSupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateSupplierStatusCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhà cung cấp.
    /// </summary>
    /// <param name="id">ID của nhà cung cấp cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa.</returns>
    [HttpDelete("{id:int}")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteSupplierCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhà cung cấp đã xóa.
    /// </summary>
    /// <param name="id">ID của nhà cung cấp cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin nhà cung cấp sau khi khôi phục.</returns>
    [HttpPost("restore/{id:int}")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreSupplierAsync(int id, CancellationToken cancellationToken)
    {
        var command = new RestoreSupplierCommand() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa nhiều nhà cung cấp cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách ID nhà cung cấp cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả xóa nhiều.</returns>
    [HttpDelete("delete-many")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSuppliersAsync(
        [FromBody] DeleteManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<DeleteManySuppliersCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Khôi phục nhiều nhà cung cấp đã xóa cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách ID nhà cung cấp cần khôi phục.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách nhà cung cấp đã được khôi phục.</returns>
    [HttpPost("restore-many")]
    [HasPermission(Suppliers.Delete)]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreSuppliersAsync(
        [FromBody] RestoreManySuppliersCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<RestoreManySuppliersCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật trạng thái cho nhiều nhà cung cấp cùng lúc.
    /// </summary>
    /// <param name="request">Danh sách ID nhà cung cấp và trạng thái mới.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách nhà cung cấp sau khi cập nhật trạng thái.</returns>
    [HttpPatch("update-status-many")]
    [HasPermission(Suppliers.Edit)]
    [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateManySupplierStatusAsync(
        [FromBody] UpdateManySupplierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateManySupplierStatusCommand>();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy thống kê số lượng nhà cung cấp theo loại đối tác.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thống kê số lượng nhà cung cấp.</returns>
    [HttpGet("statistics")]
    [RequiresAnyPermissions(Suppliers.View, Domain.Constants.Permission.Permissions.InventoryReceipts.Edit, Domain.Constants.Permission.Permissions.InventoryReceipts.Create)]
    [ProducesResponseType(typeof(SupplierStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = new GetSupplierStatisticsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xuất danh sách nhà cung cấp ra file Excel (có hỗ trợ lọc và sắp xếp).
    /// </summary>
    /// <param name="sieveModel">Các thông tin lọc, sắp xếp theo quy tắc của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>File Excel chứa danh sách nhà cung cấp.</returns>
    [HttpGet("export")]
    [HasPermission(Suppliers.View)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportSuppliersAsync(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new ExportSuppliersQuery { SieveModel = sieveModel };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
