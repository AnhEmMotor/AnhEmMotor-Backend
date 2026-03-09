using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Features.Statistical.Queries.GetAdminDashboardOverview;
using Application.Features.Statistical.Queries.GetAdminProductReport;
using Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;
using Application.Features.Statistical.Queries.GetAdminWarehouseReport;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Asp.Versioning;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using static Domain.Constants.Permission.PermissionsList;
using static Domain.Constants.Permission.PermissionsList;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Thống kê và báo cáo.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Thống kê và báo cáo")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy doanh thu theo ngày trong khoảng thời gian xác định.
    /// </summary>
    /// <param name="days">Số ngày tính từ hiện tại trở về trước</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("daily-revenue")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(IEnumerable<DailyRevenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyRevenueAsync(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyRevenueQuery() { Days = days };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy các chỉ số tổng hợp cho Dashboard.
    /// </summary>
    [HttpGet("dashboard-stats")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var query = new GetDashboardStatsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy doanh thu và lợi nhuận theo tháng.
    /// </summary>
    /// <param name="months">Số tháng tính từ hiện tại trở về trước</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("monthly-revenue-profit")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueProfitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyRevenueProfitAsync(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyRevenueProfitQuery() { Months = months };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy số lượng đơn hàng theo từng trạng thái.
    /// </summary>
    [HttpGet("order-status-counts")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(IEnumerable<OrderStatusCountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatusCountsAsync(CancellationToken cancellationToken)
    {
        var query = new GetOrderStatusCountsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy báo cáo sản phẩm của tháng trước.
    /// </summary>
    [HttpGet("product-report-last-month")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(IEnumerable<ProductReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReportLastMonthAsync(CancellationToken cancellationToken)
    {
        var query = new GetProductReportLastMonthQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy giá và tồn kho của một sản phẩm cụ thể.
    /// </summary>
    [HttpGet("product-stock-price/{variantId:int}")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(ProductStockPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStockAndPriceAsync(int variantId, CancellationToken cancellationToken)
    {
        var query = new GetProductStockAndPriceQuery() { VariantId = variantId };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ dữ liệu gộp cho Admin Dashboard.
    /// </summary>
    [HttpGet("dashboard-overview")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(AdminDashboardOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboardOverviewAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminDashboardOverviewQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy toàn bộ phân tích doanh thu cho Admin (gộp).
    /// </summary>
    [HttpGet("revenue-analysis")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(AdminRevenueAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminRevenueAnalysisAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminRevenueAnalysisQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy báo cáo hiệu suất sản phẩm cho Admin (gộp).
    /// </summary>
    [HttpGet("product-report")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(AdminProductReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminProductReportAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminProductReportQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy báo cáo tồn kho cho Admin (gộp).
    /// </summary>
    [HttpGet("warehouse-report")]
    [HasPermission(Statistical.View)]
    [ProducesResponseType(typeof(AdminWarehouseReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminWarehouseReportAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminWarehouseReportQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
