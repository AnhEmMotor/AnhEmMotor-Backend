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
using Domain.Constants.Permission.Permissions;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Th?ng k� v� b�o c�o.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Th?ng k� v� b�o c�o")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// L?y doanh thu theo ng�y trong kho?ng th?i gian x�c d?nh.
    /// </summary>
    /// <param name="days">S? ng�y t�nh t? hi?n t?i tr? v? tru?c</param>
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
    /// L?y c�c ch? s? t?ng h?p cho Dashboard.
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
    /// L?y doanh thu v� l?i nhu?n theo th�ng.
    /// </summary>
    /// <param name="months">S? th�ng t�nh t? hi?n t?i tr? v? tru?c</param>
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
    /// L?y s? lu?ng don h�ng theo t?ng tr?ng th�i.
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
    /// L?y b�o c�o s?n ph?m c?a th�ng tru?c.
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
    /// L?y gi� v� t?n kho c?a m?t s?n ph?m c? th?.
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
    /// L?y to�n b? d? li?u g?p cho Admin Dashboard.
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
    /// L?y to�n b? ph�n t�ch doanh thu cho Admin (g?p).
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
    /// L?y b�o c�o hi?u su?t s?n ph?m cho Admin (g?p).
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
    /// L?y b�o c�o t?n kho cho Admin (g?p).
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

