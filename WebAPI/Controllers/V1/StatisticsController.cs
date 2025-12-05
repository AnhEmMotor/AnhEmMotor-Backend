using Application.ApiContracts.Staticals;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Asp.Versioning;
using Domain.Helpers;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Thống kê và báo cáo.
/// </summary>
/// <param name="mediator"></param>
[ApiVersion("1.0")]
[SwaggerTag("Thống kê và báo cáo")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lấy doanh thu theo ngày trong khoảng thời gian xác định.
    /// </summary>
    /// <param name="days">Số ngày tính từ hiện tại trở về trước</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("daily-revenue")]
    [ProducesResponseType(typeof(IEnumerable<DailyRevenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyRevenue(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyRevenueQuery(days);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy các chỉ số tổng hợp cho Dashboard.
    /// </summary>
    [HttpGet("dashboard-stats")]
    [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var query = new GetDashboardStatsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy doanh thu và lợi nhuận theo tháng.
    /// </summary>
    /// <param name="months">Số tháng tính từ hiện tại trở về trước</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("monthly-revenue-profit")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueProfitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyRevenueProfit(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyRevenueProfitQuery(months);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy số lượng đơn hàng theo từng trạng thái.
    /// </summary>
    [HttpGet("order-status-counts")]
    [ProducesResponseType(typeof(IEnumerable<OrderStatusCountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatusCounts(CancellationToken cancellationToken)
    {
        var query = new GetOrderStatusCountsQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy báo cáo sản phẩm của tháng trước.
    /// </summary>
    [HttpGet("product-report-last-month")]
    [ProducesResponseType(typeof(IEnumerable<ProductReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReportLastMonth(CancellationToken cancellationToken)
    {
        var query = new GetProductReportLastMonthQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        return Ok(result);
    }

    /// <summary>
    /// Lấy giá và tồn kho của một sản phẩm cụ thể.
    /// </summary>
    [HttpGet("product-stock-price/{variantId:int}")]
    [ProducesResponseType(typeof(ProductStockPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStockAndPrice(int variantId, CancellationToken cancellationToken)
    {
        var query = new GetProductStockAndPriceQuery(variantId);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
        if(result is null)
        {
            return NotFound(
                new ErrorResponse
                {
                    Errors = [ new ErrorDetail { Message = $"Không tìm thấy sản phẩm có ID {variantId}." } ]
                });
        }
        return Ok(result);
    }
}
