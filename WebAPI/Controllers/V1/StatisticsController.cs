using Application.ApiContracts.Statistical.Responses;
using Application.Api.Contracts.Statistical.Responses;
using Application.Common.Models;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDailyRevenueDetail;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Interfaces.Repositories.Statistical;
using Application.Features.Statistical.Queries.GetAdminDashboardOverview;
using Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;
using Application.Features.Statistical.Queries.GetAdminProductReport;
using Application.Features.Statistical.Queries.GetAdminWarehouseReport;
using Application.Features.Order.Queries.GetOrderStatistics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Thống kê và báo cáo.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Thống kê và báo cáo")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticsController(IMediator mediator, IStatisticalReadRepository repository) : ApiController
{
  /// <summary>
  /// Lấy doanh thu theo ngày trong khoảng thời gian xác định.
  /// </summary>
  [HttpGet("daily-revenue")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(IEnumerable<DailyRevenueResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDailyRevenueAsync(
    [FromQuery] int days = 7,
    CancellationToken cancellationToken = default)
  {
    var query = new GetDailyRevenueQuery() { Days = days };
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy chi tiết sản phẩm và nhân viên bán trong một ngày cụ thể.
  /// </summary>
  [HttpGet("daily-revenue/detail")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(IEnumerable<DailyRevenueDetailResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDailyRevenueDetailAsync(
    [FromQuery] string reportDay,
    [FromQuery] int days = 7,
    CancellationToken cancellationToken = default)
  {
    var query = new GetDailyRevenueDetailQuery { ReportDay = reportDay, Days = days };
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy các chỉ số tổng hợp cho Dashboard.
  /// </summary>
  [HttpGet("dashboard-stats")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(DashboardStatsResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDashboardStatsAsync(CancellationToken cancellationToken)
  {
    var query = new GetDashboardStatsQuery();
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy doanh thu và lợi nhuận theo tháng.
  /// </summary>
  [HttpGet("monthly-revenue-profit")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueProfitResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMonthlyRevenueProfitAsync(
    [FromQuery] int months = 12,
    CancellationToken cancellationToken = default)
  {
    var query = new GetMonthlyRevenueProfitQuery() { Months = months };
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy số lượng đơn hàng theo từng trạng thái
  /// </summary>
  [HttpGet("order-status-counts")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(IEnumerable<OrderStatusCountResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetOrderStatusCountsAsync(CancellationToken cancellationToken)
  {
    var query = new GetOrderStatusCountsQuery();
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy báo cáo sản phẩm của tháng trước.
  /// </summary>
  [HttpGet("product-report-last-month")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(IEnumerable<ProductReportResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProductReportLastMonthAsync(CancellationToken cancellationToken)
  {
    var query = new GetProductReportLastMonthQuery();
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy giá và tồn kho của một sản phẩm cụ thể.
  /// </summary>
  [HttpGet("product-stock-price/{variantId:int}")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(ProductStockPriceResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<IActionResult> GetProductStockAndPriceAsync(int variantId, CancellationToken cancellationToken)
  {
    var query = new GetProductStockAndPriceQuery() { VariantId = variantId };
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy phân tích khách hàng.
  /// </summary>
  [HttpGet("customer-analytics")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(CustomerAnalyticsResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCustomerAnalyticsAsync(CancellationToken cancellationToken)
  {
    var result = await repository.GetCustomerAnalyticsAsync(cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Lấy báo cáo chăm sóc khách hàng.
  /// </summary>
  [HttpGet("customer-service-analytics")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(CustomerServiceAnalyticsResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken)
  {
    var result = await repository.GetCustomerServiceAnalyticsAsync(cancellationToken);
    return Ok(result);
  }

  /// <summary>
  /// Lấy thống kê tổng quan về đơn hàng (hàng đợi, SLA, lỗi, ngoại lệ).
  /// </summary>
  [HttpGet("order-statistics")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(OrderStatisticsResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetOrderStatisticsAsync(CancellationToken cancellationToken)
  {
    var query = new GetOrderStatisticsQuery();
    var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
    return HandleResult(result);
  }

  /// <summary>
  /// Lấy dữ liệu tổng quan Dashboard xưởng dịch vụ (KPI, cảnh báo, analytics).
  /// </summary>
  [HttpGet("workshop-dashboard-overview")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View,
    Permissions.Factory.DashboardManagement.View)]
  [ProducesResponseType(typeof(WorkshopDashboardResponse), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetWorkshopDashboardOverviewAsync(
    [FromQuery] string? from,
    [FromQuery] string? to,
    CancellationToken cancellationToken)
  {
    return Ok(new WorkshopDashboardResponse());
  }
}
