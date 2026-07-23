using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Features.InventoryReports.Queries.ExportInventoryReport;
using Application.Features.InventoryReports.Queries.GetInventoryReportDetail;
using Application.Features.InventoryReports.Queries.GetInventoryReportSummary;
using Domain.Primitives;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Báo cáo tồn kho — cung cấp tổng hợp và chi tiết giao dịch tồn kho.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class InventoryReportController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy báo cáo tổng hợp tồn kho — số lượng, giá vốn, trạng thái theo biến thể sản phẩm.
    /// </summary>
    /// <param name="query">Tham số truy vấn (theo quy tắc của Sieve: trang, lọc, sắp xếp).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách báo cáo tồn kho tổng hợp.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InventoryReportSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReportSummaryAsync(
        [FromQuery] GetInventoryReportSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xuất báo cáo tồn kho tổng hợp ra file Excel.
    /// </summary>
    /// <param name="query">Tham số truy vấn (theo quy tắc của Sieve: lọc, sắp xếp).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>File Excel (.xlsx) chứa báo cáo tồn kho.</returns>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportInventoryReportAsync(
        [FromQuery] ExportInventoryReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xem chi tiết báo cáo tồn kho cho một biến thể sản phẩm cụ thể — lịch sử nhập/xuất, số dư.
    /// </summary>
    /// <param name="variantId">ID của biến thể sản phẩm cần xem chi tiết.</param>
    /// <param name="colorId">ID màu sắc lọc (tùy chọn — null để xem tất cả màu).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chi tiết báo cáo tồn kho của biến thể.</returns>
    [HttpGet("details")]
    [ProducesResponseType(typeof(InventoryReportDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventoryReportDetailAsync(
        [FromQuery] int variantId,
        [FromQuery] int? colorId,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryReportDetailQuery { VariantId = variantId, ColorId = colorId };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
