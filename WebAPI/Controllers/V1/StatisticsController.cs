using Application.Api.Contracts.Statistical.Responses;
using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Features.Order.Queries.GetOrderStatistics;
using Application.Features.Statistical.Queries.GetAdminDashboardOverview;
using Application.Features.Statistical.Queries.GetAdminProductReport;
using Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;
using Application.Features.Statistical.Queries.GetAdminWarehouseReport;
using Application.Features.Statistical.Queries.GetDailyCategoryRevenue;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDailyRevenueDetail;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Features.Statistical.Queries.GetRevenueByCategory;
using Application.Interfaces.Repositories.Statistical;

using Asp.Versioning;
using Domain.Constants.Order;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Thống kê và báo cáo — cung cấp dữ liệu tổng hợp, biểu đồ và phân tích cho các phân hệ Admin, Accountant, Factory.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Thống kê và báo cáo")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class StatisticsController(
    IMediator mediator,
    IStatisticalReadRepository repository,
    ApplicationDBContext dbContext) : ApiController
{
    /// <summary>
    /// Lấy doanh thu theo ngày trong khoảng thời gian xác định.
    /// </summary>
    /// <param name="days">Số ngày lấy dữ liệu doanh thu trở lại (mặc định: 7 ngày).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách doanh thu theo từng ngày.</returns>
    /// <response code="200">Trả về danh sách doanh thu hàng ngày thành công.</response>
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
    /// Lấy chi tiết sản phẩm và nhân viên bán hàng trong một ngày cụ thể.
    /// </summary>
    /// <param name="reportDay">Ngày báo cáo (định dạng dd/MM/yyyy hoặc ISO).</param>
    /// <param name="days">Số ngày lùi về để lấy bối cảnh doanh thu (mặc định: 7 ngày).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Chi tiết doanh thu theo ngày gồm sản phẩm và nhân viên bán hàng.</returns>
    /// <response code="200">Trả về chi tiết doanh thu thành công.</response>
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
    /// Lấy các chỉ số tổng hợp cho Dashboard (tổng đơn hàng, doanh thu, khách hàng mới).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu tổng hợp Dashboard.</returns>
    /// <response code="200">Trả về dữ liệu tổng hợp Dashboard thành công.</response>
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
    /// Lấy doanh thu và lợi nhuận theo tháng (biểu đồ xu hướng).
    /// </summary>
    /// <param name="months">Số tháng lấy dữ liệu xu hướng doanh thu (mặc định: 12 tháng).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách doanh thu và lợi nhuận theo từng tháng.</returns>
    /// <response code="200">Trả về doanh thu theo tháng thành công.</response>
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
    /// Lấy số lượng đơn hàng theo từng trạng thái (để biểu đồ tròn / donut chart).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách số lượng đơn hàng theo từng trạng thái.</returns>
    /// <response code="200">Trả về số lượng đơn hàng theo trạng thái thành công.</response>
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
    /// Lấy báo cáo sản phẩm bán chạy của tháng trước (top sản phẩm, doanh số, tồn kho).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách sản phẩm bán chạy của tháng trước.</returns>
    /// <response code="200">Trả về báo cáo sản phẩm tháng trước thành công.</response>
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
    /// Lấy báo cáo sản phẩm tổng hợp (doanh số, tồn kho, phân bố thương hiệu).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Báo cáo tổng hợp về sản phẩm.</returns>
    /// <response code="200">Trả về báo cáo sản phẩm thành công.</response>
    [HttpGet("product-report")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminProductReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminProductReportAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminProductReportQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy báo cáo tổng quan kho (tồn kho hiện tại, giá vốn, cảnh báo sản phẩm sắp hết).
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu lọc dữ liệu (tuỳ chọn, ISO 8601).</param>
    /// <param name="endDate">Ngày kết thúc lọc dữ liệu (tuỳ chọn, ISO 8601).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Báo cáo tổng quan kho hàng.</returns>
    /// <response code="200">Trả về báo cáo kho thành công.</response>
    [HttpGet("warehouse-report")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminWarehouseReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminWarehouseReportAsync(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetAdminWarehouseReportQuery { StartDate = startDate, EndDate = endDate };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy phân tích doanh thu chi tiết theo kênh bán (online/offline), khu vực và nhân viên.
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu phân tích (tuỳ chọn, ISO 8601).</param>
    /// <param name="endDate">Ngày kết thúc phân tích (tuỳ chọn, ISO 8601).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu phân tích doanh thu chi tiết.</returns>
    /// <response code="200">Trả về phân tích doanh thu thành công.</response>
    [HttpGet("revenue-analysis")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminRevenueAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminRevenueAnalysisAsync(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetAdminRevenueAnalysisQuery { StartDate = startDate, EndDate = endDate };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy giá bán hiện tại và số lượng tồn kho của một biến thể sản phẩm cụ thể.
    /// </summary>
    /// <param name="variantId">ID của biến thể sản phẩm (ProductVariant) cần xem giá và tồn kho.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin giá và tồn kho của biến thể sản phẩm.</returns>
    /// <response code="200">Trả về thông tin giá và tồn kho thành công.</response>
    /// <response code="404">Không tìm thấy biến thể sản phẩm.</response>
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
    /// Lấy phân tích khách hàng (tỷ lệ giữ chân, LTV, phân bố nguồn khách).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu phân tích khách hàng.</returns>
    /// <response code="200">Trả về phân tích khách hàng thành công.</response>
    [HttpGet("customer-analytics")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(CustomerAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAnalyticsAsync(CancellationToken cancellationToken)
    {
        var result = await repository.GetCustomerAnalyticsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy báo cáo chăm sóc khách hàng (tốc độ phản hồi, tỷ lệ giải quyết, đánh giá).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu báo cáo chăm sóc khách hàng.</returns>
    /// <response code="200">Trả về báo cáo chăm sóc khách hàng thành công.</response>
    [HttpGet("customer-service-analytics")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(CustomerServiceAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken)
    {
        var result = await repository.GetCustomerServiceAnalyticsAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thống kê tổng quan về đơn hàng (hàng đợi, SLA, lỗi, ngoại lệ).
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu thống kê đơn hàng chi tiết.</returns>
    /// <response code="200">Trả về thống kê đơn hàng thành công.</response>
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
    /// Lấy dữ liệu tổng quan Dashboard dành cho Kế toán (summary, doanh thu, đơn hàng gần đây).
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu phân tích (tuỳ chọn, ISO 8601).</param>
    /// <param name="endDate">Ngày kết thúc phân tích (tuỳ chọn, ISO 8601).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu tổng quan Dashboard cho Kế toán.</returns>
    /// <response code="200">Trả về tổng quan Dashboard thành công.</response>
    [HttpGet("dashboard-overview")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminDashboardOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboardOverviewAsync(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var query = new GetAdminDashboardOverviewQuery { StartDate = startDate, EndDate = endDate };
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy dữ liệu tổng quan Dashboard xưởng dịch vụ (KPI, cảnh báo, doanh thu xưởng so với bán lẻ).
    /// </summary>
    /// <param name="from">Thời điểm bắt đầu phân tích (chuỗi ngày tháng, tuỳ chọn, để trống lấy đầu tháng hiện tại).</param>
    /// <param name="to">Thời điểm kết thúc phân tích (chuỗi ngày tháng, tuỳ chọn, để trống lấy thời điểm hiện tại).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu tổng quan Dashboard xưởng dịch vụ (KPI, biểu đồ, danh sách phiếu sửa chữa đang xử lý).</returns>
    /// <response code="200">Trả về tổng quan xưởng dịch vụ thành công.</response>
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
        var response = await repository.GetWorkshopDashboardOverviewAsync(
            from ?? string.Empty,
            to ?? string.Empty,
            cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    /// <summary>
    /// Lấy dữ liệu báo cáo xưởng dịch vụ dành cho phân hệ Kế toán (KPI, phiếu đang sửa chữa).
    /// </summary>
    /// <param name="start">Thời điểm bắt đầu lọc dữ liệu (ISO 8601, tuỳ chọn).</param>
    /// <param name="end">Thời điểm kết thúc lọc dữ liệu (ISO 8601, tuỳ chọn).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Báo cáo xưởng dịch vụ gồm KPI và danh sách phiếu sửa chữa trong kỳ.</returns>
    /// <response code="200">Trả về báo cáo xưởng dịch vụ thành công.</response>
    [HttpGet("workshop-overview")]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkshopOverviewAsync(
        [FromQuery] DateTimeOffset? start,
        [FromQuery] DateTimeOffset? end,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var periodStart = start ?? new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = end ?? now;
        var inProgressCount = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .CountAsync(m => m.TotalCost == 0, cancellationToken)
            .ConfigureAwait(false);
        var workshopRevenue = await dbContext.WorkshopPayments
            .IgnoreQueryFilters()
            .Where(p => p.CreatedAt >= periodStart && p.CreatedAt <= periodEnd)
            .SumAsync(p => p.TotalAmount, cancellationToken)
            .ConfigureAwait(false);
        var overdueCutoff = now.AddHours(-48);
        var overdueCount = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .CountAsync(m => m.TotalCost == 0 && m.CreatedAt <= overdueCutoff, cancellationToken)
            .ConfigureAwait(false);
        var activeOrders = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .Where(m => m.TotalCost == 0)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .Select(
                m => new
                {
                    m.Id,
                    m.MaintenanceNumber,
                    m.VehicleId,
                    m.Description,
                    m.LaborCost,
                    m.PartsCost,
                    m.CreatedAt,
                    m.TechnicianId
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var vehicleIds = activeOrders.Select(o => o.VehicleId).Distinct().ToList();
        var vehicleDict = new Dictionary<int, string>();
        var customerDict = new Dictionary<int, string>();
        var empDict = new Dictionary<int, string>();
        if (vehicleIds.Count > 0)
        {
            var vehicleList = await dbContext.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.User)
                .Include(v => v.Lead)
                .Where(v => vehicleIds.Contains(v.Id))
                .Select(
                    v => new
                    {
                        v.Id,
                        VehicleInfo = !string.IsNullOrEmpty(v.LicensePlate) ? v.LicensePlate : v.VinNumber,
                        CustomerName = v.User != null ? v.User.FullName : (v.Lead != null ? v.Lead.FullName : "-")
                    })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            vehicleDict = vehicleList.ToDictionary(x => x.Id, x => x.VehicleInfo);
            customerDict = vehicleList.ToDictionary(x => x.Id, x => x.CustomerName);
            empDict = await dbContext.EmployeeProfiles
                .IgnoreQueryFilters()
                .Include(e => e.User)
                .ToDictionaryAsync(
                    e => e.Id,
                    e => e.User != null ? e.User.FullName : "Chưa phân công",
                    cancellationToken)
                .ConfigureAwait(false);
        }
        var vehicleFees = activeOrders.Select(o => new { o.Id, o.VehicleId, Fee = o.LaborCost + o.PartsCost }).ToList();
        var vehicleFeeDict = vehicleFees.ToDictionary(x => x.Id, x => x.Fee);
        var repairOrders = activeOrders.Select(
            o => new
            {
                id = o.Id,
                orderCode = o.MaintenanceNumber,
                customerName = customerDict.TryGetValue(o.VehicleId, out var cn) ? cn : "-",
                vehicleInfo = vehicleDict.TryGetValue(o.VehicleId, out var vi) ? vi : "-",
                technicianName = (o.TechnicianId is int tid && empDict.TryGetValue(tid, out var tn))
                    ? tn
                    : "Chưa phân công",
                status = "Đang sửa chữa",
                startedAt = o.CreatedAt,
                laborFee = vehicleFeeDict.TryGetValue(o.Id, out var f) ? f : 0m
            })
            .ToList();
        var chartData = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var startOfMonth = new DateTimeOffset(monthDate.Year, monthDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
            var monthWorkshopRev = await dbContext.WorkshopPayments
                .IgnoreQueryFilters()
                .Where(p => p.CreatedAt >= startOfMonth && p.CreatedAt <= endOfMonth)
                .SumAsync(p => p.TotalAmount, cancellationToken)
                .ConfigureAwait(false);
            var monthRetailRev = await dbContext.OutputOrders
                .IgnoreQueryFilters()
                .Where(
                    o => o.CreatedAt >= startOfMonth && o.CreatedAt <= endOfMonth && o.StatusId == OrderStatus.Completed)
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false);
            chartData.Add(
                new
                {
                    month = monthDate.ToString("MM/yyyy"),
                    workshopRevenue = monthWorkshopRev,
                    retailRevenue = monthRetailRev
                });
        }
        return Ok(
            new
            {
                kpi = new { inProgressCount, avgCompletionHours = 2.5, monthlyRevenue = workshopRevenue, overdueCount },
                revenueComparisonChart = chartData,
                repairOrders
            });
    }

    /// <summary>
    /// Thống kê chi tiết các loại hóa đơn trong đơn hàng (kênh online/offline, phương thức thanh toán, danh mục).
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu lọc (định dạng dd/MM/yyyy).</param>
    /// <param name="endDate">Ngày kết thúc lọc (định dạng dd/MM/yyyy).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Báo cáo tổng quan hóa đơn gồm KPI, xu hướng, phân bố sản phẩm và danh sách hóa đơn.</returns>
    /// <response code="200">Trả về thống kê hóa đơn thành công.</response>
    [HttpGet("invoice-overview")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(InvoiceOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceOverviewAsync(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.Parse(startDate);
        var end = DateTimeOffset.Parse(endDate).AddDays(1).AddTicks(-1);
        var ordersQuery = dbContext.OutputOrders
            .IgnoreQueryFilters()
            .Include(o => o.OutputInfos)
            .ThenInclude(oi => oi.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end);
        var orders = await ordersQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        decimal totalInvoiced = 0;
        decimal collectedCash = 0;
        decimal pendingTransit = 0;
        decimal canceledAmount = 0;
        var trendDataDict = new Dictionary<string, (decimal Offline, decimal Online)>();
        var productDataDict = new Dictionary<string, decimal>();
        var paymentDataDict = new Dictionary<string, decimal>();
        var invoicesData = new List<InvoiceListItem>();
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            trendDataDict[d.ToString("dd/MM")] = (0, 0);
        }
        foreach (var o in orders)
        {
            decimal orderTotal = o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0;
            if (o.StatusId != OrderStatus.Cancelled)
            {
                totalInvoiced += orderTotal;
                if (o.StatusId == OrderStatus.Completed || o.PaymentStatus == "paid")
                    collectedCash += orderTotal;
                else if (o.StatusId == OrderStatus.Delivering || o.PaymentMethod == "cod")
                    pendingTransit += orderTotal;
            } else
            {
                canceledAmount += orderTotal;
            }
            var dayStr = o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("dd/MM") : string.Empty;
            bool isOnline = o.CreatedBy == null || o.LeadId != null;
            if (trendDataDict.ContainsKey(dayStr))
            {
                var cur = trendDataDict[dayStr];
                if (isOnline)
                    trendDataDict[dayStr] = (cur.Offline, cur.Online + orderTotal);
                else
                    trendDataDict[dayStr] = (cur.Offline + orderTotal, cur.Online);
            }
            string mainCategory = "Khác";
            if (o.OutputInfos != null && o.OutputInfos.Any())
            {
                var firstCat = o.OutputInfos.FirstOrDefault()?.ProductVariant?.Product?.ProductCategory?.Name;
                if (!string.IsNullOrEmpty(firstCat))
                    mainCategory = firstCat;
                foreach (var oi in o.OutputInfos)
                {
                    var catName = oi.ProductVariant?.Product?.ProductCategory?.Name ?? "Khác";
                    var itemTotal = (oi.Price ?? 0) * (oi.Count ?? 0);
                    if (productDataDict.ContainsKey(catName))
                        productDataDict[catName] += itemTotal;
                    else
                        productDataDict[catName] = itemTotal;
                }
            }
            var payMethod = string.IsNullOrEmpty(o.PaymentMethod) ? "Khác" : o.PaymentMethod;
            if (paymentDataDict.ContainsKey(payMethod))
                paymentDataDict[payMethod] += orderTotal;
            else
                paymentDataDict[payMethod] = orderTotal;
            var items = new List<InvoiceListItemPart>();
            string vName = string.Empty, vVin = string.Empty, vEngine = string.Empty;
            if (o.OutputInfos != null)
            {
                foreach (var oi in o.OutputInfos)
                {
                    items.Add(
                        new InvoiceListItemPart(
                            oi.Count ?? 0,
                            oi.ProductVariant?.Product?.Name ?? string.Empty,
                            oi.ProductVariant?.SKU ?? string.Empty));
                    if (mainCategory.Contains("Xe") && string.IsNullOrEmpty(vName))
                    {
                        vName = oi.ProductVariant?.Product?.Name ?? string.Empty;
                    }
                }
            }
            invoicesData.Add(
                new InvoiceListItem(
                    $"HD{o.Id:D5}",
                    o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    isOnline ? "Online" : "Offline",
                    mainCategory,
                    payMethod,
                    orderTotal,
                    o.StatusId == OrderStatus.Completed
                        ? "Đã thu đủ"
                        : (o.StatusId == OrderStatus.Cancelled ? "Đã hủy" : "Chờ đối soát"),
                    new InvoiceListItemDetails(
                        o.CustomerName ?? "-",
                        o.CustomerPhone ?? "-",
                        vName,
                        vVin,
                        vEngine,
                        "Nội bộ",
                        "-",
                        items)));
        }
        var trendDataList = trendDataDict
            .Select(kvp => new InvoiceTrendData(kvp.Key, kvp.Value.Offline, kvp.Value.Online))
            .ToList();
        var productDataList = productDataDict.Select(kvp => new InvoiceProductData(kvp.Key, kvp.Value)).ToList();
        var paymentDataList = paymentDataDict.Select(kvp => new InvoicePaymentData(kvp.Key, kvp.Value)).ToList();
        return Ok(
            new InvoiceOverviewResponse(
                new InvoiceOverviewKpi(totalInvoiced, collectedCash, pendingTransit, canceledAmount),
                trendDataList,
                productDataList,
                paymentDataList,
                invoicesData.OrderByDescending(x => x.Date).ToList()));
    }

    /// <summary>
    /// Thống kê hợp đồng tổng hợp (Bán xe và Nhà cung cấp) trong khoảng thời gian.
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu lọc (định dạng dd/MM/yyyy).</param>
    /// <param name="endDate">Ngày kết thúc lọc (định dạng dd/MM/yyyy).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Báo cáo hợp đồng gồm KPI, xu hướng, trạng thái, top nhà cung cấp, danh sách hợp đồng.</returns>
    /// <response code="200">Trả về thống kê hợp đồng thành công.</response>
    [HttpGet("contract-overview")]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View)]
    [ProducesResponseType(typeof(ContractOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContractOverviewAsync(
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.Parse(startDate);
        var end = DateTimeOffset.Parse(endDate).AddDays(1).AddTicks(-1);
        var salesContracts = await dbContext.SalesContracts
            .IgnoreQueryFilters()
            .Where(c => c.CreatedAt >= start && c.CreatedAt <= end)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var supplierContracts = await dbContext.SupplierContracts
            .IgnoreQueryFilters()
            .Include(c => c.Supplier)
            .Where(c => c.CreatedAt >= start && c.CreatedAt <= end)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        int totalSalesCount = salesContracts.Count;
        decimal totalSalesValue = salesContracts.Sum(c => c.ActualSalePrice);
        int totalSupplierCount = supplierContracts.Count;
        decimal totalSupplierValue = supplierContracts.Sum(c => c.ContractValue);
        var trendDataDict = new Dictionary<string, (decimal Sales, decimal Supplier)>();
        var statusDataDict = new Dictionary<string, int>();
        var topSupplierDict = new Dictionary<string, decimal>();
        var contractsData = new List<ContractListItem>();
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            trendDataDict[d.ToString("dd/MM")] = (0, 0);
        }
        foreach (var sc in salesContracts)
        {
            var dayStr = sc.CreatedAt?.ToString("dd/MM") ?? start.ToString("dd/MM");
            if (trendDataDict.ContainsKey(dayStr))
            {
                var cur = trendDataDict[dayStr];
                trendDataDict[dayStr] = (cur.Sales + sc.ActualSalePrice, cur.Supplier);
            }
            var statusName = sc.Status ?? "Nháp";
            if (statusDataDict.ContainsKey(statusName))
                statusDataDict[statusName]++;
            else
                statusDataDict[statusName] = 1;
            contractsData.Add(
                new ContractListItem(
                    sc.Id.ToString(),
                    sc.ContractNumber ?? "HD-BX-...",
                    "Bán xe",
                    sc.CustomerFullName ?? "Khách lẻ",
                    sc.ActualSalePrice,
                    statusName,
                    sc.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty));
        }
        foreach (var supc in supplierContracts)
        {
            var dayStr = supc.CreatedAt?.ToString("dd/MM") ?? start.ToString("dd/MM");
            if (trendDataDict.ContainsKey(dayStr))
            {
                var cur = trendDataDict[dayStr];
                trendDataDict[dayStr] = (cur.Sales, cur.Supplier + supc.ContractValue);
            }
            var statusName = supc.Status ?? "Draft";
            if (statusDataDict.ContainsKey(statusName))
                statusDataDict[statusName]++;
            else
                statusDataDict[statusName] = 1;
            var supplierName = supc.Supplier?.Name ?? "NCC Khác";
            if (topSupplierDict.ContainsKey(supplierName))
                topSupplierDict[supplierName] += supc.ContractValue;
            else
                topSupplierDict[supplierName] = supc.ContractValue;
            contractsData.Add(
                new ContractListItem(
                    supc.Id.ToString(),
                    supc.ContractNumber ?? "HD-NCC-...",
                    "Nhà cung cấp",
                    supplierName,
                    supc.ContractValue,
                    statusName,
                    supc.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty));
        }
        var trendDataList = trendDataDict
            .Select(kvp => new ContractTrendData(kvp.Key, kvp.Value.Sales, kvp.Value.Supplier))
            .ToList();
        var statusDataList = statusDataDict.Select(kvp => new ContractStatusData(kvp.Key, kvp.Value)).ToList();
        var topSuppliersList = topSupplierDict
            .OrderByDescending(x => x.Value)
            .Take(5)
            .Select(kvp => new ContractTopSupplierData(kvp.Key, kvp.Value))
            .ToList();
        return Ok(
            new ContractOverviewResponse(
                new ContractOverviewKpi(totalSalesCount, totalSalesValue, totalSupplierCount, totalSupplierValue),
                trendDataList,
                statusDataList,
                topSuppliersList,
                contractsData.OrderByDescending(x => x.Date).ToList()));
    }

    /// <summary>
    /// Lấy doanh thu phân theo danh mục sản phẩm trong khoảng thời gian.
    /// </summary>
    /// <param name="start">Thời điểm bắt đầu (ISO 8601).</param>
    /// <param name="end">Thời điểm kết thúc (ISO 8601).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách doanh thu theo từng danh mục sản phẩm.</returns>
    /// <response code="200">Trả về doanh thu theo danh mục thành công.</response>
    [HttpGet("revenue-by-category")]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View)]
    [ProducesResponseType(typeof(IEnumerable<RevenueByCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueByCategoryAsync(
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        CancellationToken cancellationToken)
    {
        var query = new GetRevenueByCategoryQuery(start, end);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy doanh thu theo ngày và danh mục sản phẩm (multi-series cho biểu đồ).
    /// </summary>
    /// <param name="days">Số ngày lấy dữ liệu xu hướng (mặc định: 7 ngày).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Doanh thu theo ngày và danh mục sản phẩm (multi-series).</returns>
    /// <response code="200">Trả về doanh thu theo ngày và danh mục thành công.</response>
    [HttpGet("daily-category-revenue")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(IEnumerable<DailyCategoryRevenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyCategoryRevenueAsync(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyCategoryRevenueQuery(days);
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
