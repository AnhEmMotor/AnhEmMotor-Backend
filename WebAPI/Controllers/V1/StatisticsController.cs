using Application.ApiContracts.Statistical.Responses;
using Application.Api.Contracts.Statistical.Responses;
using Application.Common.Models;
using Domain.Constants.Order;
using Application.Features.Statistical.Queries.GetDailyRevenue;
using Application.Features.Statistical.Queries.GetDailyRevenueDetail;
using Application.Features.Statistical.Queries.GetDashboardStats;
using Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;
using Application.Features.Statistical.Queries.GetOrderStatusCounts;
using Application.Features.Statistical.Queries.GetProductReportLastMonth;
using Application.Features.Statistical.Queries.GetProductStockAndPrice;
using Application.Features.Statistical.Queries.GetWorkshopDashboardOverview;
using Application.Interfaces.Repositories.Statistical;
using Application.Features.Statistical.Queries.GetAdminDashboardOverview;
using Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;
using Application.Features.Statistical.Queries.GetAdminProductReport;
using Application.Features.Statistical.Queries.GetAdminWarehouseReport;
using Application.Features.Statistical.Queries.GetRevenueByCategory;
using Application.Features.Statistical.Queries.GetDailyCategoryRevenue;
using Application.ApiContracts.Statistical.Responses;
using Application.Features.Order.Queries.GetOrderStatistics;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Entities;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
public class StatisticsController(IMediator mediator, IStatisticalReadRepository repository, Infrastructure.DBContexts.ApplicationDBContext dbContext) : ApiController
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
    /// Lấy báo cáo sản phẩm tổng hợp (doanh số, tồn kho, phân bố thương hiệu).
    /// </summary>
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
    /// Lấy báo cáo tổng quan kho (tồn kho, giá vốn, cảnh báo).
    /// </summary>
    [HttpGet("warehouse-report")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminWarehouseReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminWarehouseReportAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminWarehouseReportQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy phân tích doanh thu chi tiết (theo kênh, khu vực, nhân viên).
    /// </summary>
    [HttpGet("revenue-analysis")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminRevenueAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminRevenueAnalysisAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminRevenueAnalysisQuery();
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
        var result = await repository.GetCustomerAnalyticsAsync(cancellationToken).ConfigureAwait(false);
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
        var result = await repository.GetCustomerServiceAnalyticsAsync(cancellationToken).ConfigureAwait(false);
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
    /// Lấy dữ liệu tổng quan Dashboard kế toán (summary, doanh thu, đơn hàng gần đây).
    /// </summary>
    [HttpGet("dashboard-overview")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View,
        Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(AdminDashboardOverviewResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboardOverviewAsync(CancellationToken cancellationToken)
    {
        var query = new GetAdminDashboardOverviewQuery();
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
        var response = await repository.GetWorkshopDashboardOverviewAsync(
            from ?? "", to ?? "", cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    /// <summary>
    /// Lấy dữ liệu báo cáo xưởng dịch vụ cho phân hệ Kế toán (KPI + phiếu đang sửa chữa).
    /// </summary>
    [HttpGet("workshop-overview")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View)]
    public async Task<IActionResult> GetWorkshopOverviewAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var inProgressCount = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .CountAsync(m => m.TotalCost >= 0, cancellationToken)
            .ConfigureAwait(false);

        var workshopRevenue = await dbContext.WorkshopPayments
            .IgnoreQueryFilters()
            .Where(p => p.CreatedAt >= monthStart)
            .SumAsync(p => p.TotalAmount, cancellationToken)
            .ConfigureAwait(false);

        var twoHoursAgo = now.AddHours(-2);
        var overdueCount = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .CountAsync(m => m.TotalCost >= 0 && m.CreatedAt <= twoHoursAgo, cancellationToken)
            .ConfigureAwait(false);

        var activeOrders = await dbContext.MaintenanceHistory
            .IgnoreQueryFilters()
            .Where(m => m.TotalCost >= 0)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .Select(m => new { m.Id, m.MaintenanceNumber, m.VehicleId, m.Description, m.LaborCost, m.PartsCost, m.CreatedAt, m.TechnicianId })
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
                .Select(v => new { 
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
                .ToDictionaryAsync(e => e.Id, e => e.User != null ? e.User.FullName : "Chưa phân công", cancellationToken)
                .ConfigureAwait(false);
        }

        var vehicleFees = activeOrders.Select(o => new
        {
            o.Id,
            o.VehicleId,
            Fee = o.LaborCost + o.PartsCost
        }).ToList();
        var vehicleFeeDict = vehicleFees.ToDictionary(x => x.Id, x => x.Fee);

        var repairOrders = activeOrders.Select(o => new
        {
            id = o.Id,
            orderCode = o.MaintenanceNumber,
            customerName = customerDict.TryGetValue(o.VehicleId, out var cn) ? cn : "-",
            vehicleInfo = vehicleDict.TryGetValue(o.VehicleId, out var vi) ? vi : "-",
            technicianName = (o.TechnicianId.HasValue && empDict.TryGetValue(o.TechnicianId.Value, out var tn)) ? tn : "Chưa phân công",
            status = "Đang sửa chữa",
            startedAt = o.CreatedAt,
            laborFee = vehicleFeeDict.TryGetValue(o.Id, out var f) ? f : 0m
        }).ToList();

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
                .Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt <= endOfMonth && o.StatusId == Domain.Constants.Order.OrderStatus.Completed)
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false);

            chartData.Add(new
            {
                month = monthDate.ToString("MM/yyyy"),
                workshopRevenue = monthWorkshopRev,
                retailRevenue = monthRetailRev
            });
        }

        return Ok(new
        {
            kpi = new { inProgressCount, avgCompletionHours = 2.5, monthlyRevenue = workshopRevenue, overdueCount },
            revenueComparisonChart = chartData,
            repairOrders
        });
    }

    /// <summary>
    /// Thống kê chi tiết các loại hóa đơn trong đơn hàng (OutputOrders).
    /// </summary>
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
            .ThenInclude(pv => pv.Product)
            .ThenInclude(p => p.ProductCategory)
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

        // Init trend dictionary
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            trendDataDict[d.ToString("dd/MM")] = (0, 0);
        }

        foreach (var o in orders)
        {
            decimal orderTotal = o.OutputInfos?.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)) ?? 0;

            // KPIs
            if (o.StatusId != OrderStatus.Cancelled)
            {
                totalInvoiced += orderTotal;
                if (o.StatusId == OrderStatus.Completed || o.PaymentStatus == "paid")
                    collectedCash += orderTotal;
                else if (o.StatusId == OrderStatus.Delivering || o.PaymentMethod == "cod")
                    pendingTransit += orderTotal;
            }
            else
            {
                canceledAmount += orderTotal;
            }

            // Channels (Trend)
            var dayStr = o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("dd/MM") : string.Empty;
            bool isOnline = o.CreatedBy == null || o.LeadId != null; // Simplification
            if (trendDataDict.ContainsKey(dayStr))
            {
                var cur = trendDataDict[dayStr];
                if (isOnline) trendDataDict[dayStr] = (cur.Offline, cur.Online + orderTotal);
                else trendDataDict[dayStr] = (cur.Offline + orderTotal, cur.Online);
            }

            // Category & Product
            string mainCategory = "Khác";
            if (o.OutputInfos != null && o.OutputInfos.Any())
            {
                var firstCat = o.OutputInfos.FirstOrDefault()?.ProductVariant?.Product?.ProductCategory?.Name;
                if (!string.IsNullOrEmpty(firstCat)) mainCategory = firstCat;

                foreach (var oi in o.OutputInfos)
                {
                    var catName = oi.ProductVariant?.Product?.ProductCategory?.Name ?? "Khác";
                    var itemTotal = (oi.Price ?? 0) * (oi.Count ?? 0);
                    if (productDataDict.ContainsKey(catName)) productDataDict[catName] += itemTotal;
                    else productDataDict[catName] = itemTotal;
                }
            }

            // Payment Methods
            var payMethod = string.IsNullOrEmpty(o.PaymentMethod) ? "Khác" : o.PaymentMethod;
            if (paymentDataDict.ContainsKey(payMethod)) paymentDataDict[payMethod] += orderTotal;
            else paymentDataDict[payMethod] = orderTotal;

            // Invoice List Item
            var items = new List<InvoiceListItemPart>();
            string vName = "", vVin = "", vEngine = "";
            if (o.OutputInfos != null)
            {
                foreach (var oi in o.OutputInfos)
                {
                    items.Add(new InvoiceListItemPart(oi.Count ?? 0, oi.ProductVariant?.Product?.Name ?? "", oi.ProductVariant?.SKU ?? ""));
                    if (mainCategory.Contains("Xe") && string.IsNullOrEmpty(vName))
                    {
                        vName = oi.ProductVariant?.Product?.Name ?? "";
                    }
                }
            }

            invoicesData.Add(new InvoiceListItem(
                $"HD{o.Id:D5}",
                o.CreatedAt.HasValue ? o.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                isOnline ? "Online" : "Offline",
                mainCategory,
                payMethod,
                orderTotal,
                o.StatusId == OrderStatus.Completed ? "Đã thu đủ" : (o.StatusId == OrderStatus.Cancelled ? "Đã hủy" : "Chờ đối soát"),
                new InvoiceListItemDetails(
                    o.CustomerName ?? "-",
                    o.CustomerPhone ?? "-",
                    vName,
                    vVin,
                    vEngine,
                    "Nội bộ",
                    "-",
                    items
                )
            ));
        }

        var trendDataList = trendDataDict.Select(kvp => new InvoiceTrendData(kvp.Key, kvp.Value.Offline, kvp.Value.Online)).ToList();
        var productDataList = productDataDict.Select(kvp => new InvoiceProductData(kvp.Key, kvp.Value)).ToList();
        var paymentDataList = paymentDataDict.Select(kvp => new InvoicePaymentData(kvp.Key, kvp.Value)).ToList();

        return Ok(new InvoiceOverviewResponse(
            new InvoiceOverviewKpi(totalInvoiced, collectedCash, pendingTransit, canceledAmount),
            trendDataList,
            productDataList,
            paymentDataList,
            invoicesData.OrderByDescending(x => x.Date).ToList()
        ));
    }

    /// <summary>
    /// Thống kê hợp đồng tổng hợp (Bán xe & Nhà cung cấp).
    /// </summary>
    [HttpGet("contract-overview")]
    [RequiresAnyPermissions(
        Permissions.Admin.DashboardManagement.View,
        Permissions.Accountant.DashboardManagement.View)]
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
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var supplierContracts = await dbContext.SupplierContracts
            .IgnoreQueryFilters()
            .Include(c => c.Supplier)
            .Where(c => c.CreatedAt >= start && c.CreatedAt <= end)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

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
            if (statusDataDict.ContainsKey(statusName)) statusDataDict[statusName]++;
            else statusDataDict[statusName] = 1;

            contractsData.Add(new ContractListItem(
                sc.Id.ToString(),
                sc.ContractNumber ?? "HD-BX-...",
                "Bán xe",
                sc.CustomerFullName ?? "Khách lẻ",
                sc.ActualSalePrice,
                statusName,
                sc.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            ));
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
            if (statusDataDict.ContainsKey(statusName)) statusDataDict[statusName]++;
            else statusDataDict[statusName] = 1;

            var supplierName = supc.Supplier?.Name ?? "NCC Khác";
            if (topSupplierDict.ContainsKey(supplierName)) topSupplierDict[supplierName] += supc.ContractValue;
            else topSupplierDict[supplierName] = supc.ContractValue;

            contractsData.Add(new ContractListItem(
                supc.Id.ToString(),
                supc.ContractNumber ?? "HD-NCC-...",
                "Nhà cung cấp",
                supplierName,
                supc.ContractValue,
                statusName,
                supc.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
            ));
        }

        var trendDataList = trendDataDict.Select(kvp => new ContractTrendData(kvp.Key, kvp.Value.Sales, kvp.Value.Supplier)).ToList();
        var statusDataList = statusDataDict.Select(kvp => new ContractStatusData(kvp.Key, kvp.Value)).ToList();
        var topSuppliersList = topSupplierDict.OrderByDescending(x => x.Value).Take(5)
            .Select(kvp => new ContractTopSupplierData(kvp.Key, kvp.Value)).ToList();

        return Ok(new ContractOverviewResponse(
            new ContractOverviewKpi(totalSalesCount, totalSalesValue, totalSupplierCount, totalSupplierValue),
            trendDataList,
            statusDataList,
            topSuppliersList,
            contractsData.OrderByDescending(x => x.Date).ToList()
        ));
    }

  /// <summary>
  /// Lấy doanh thu phân theo danh mục sản phẩm.
  /// </summary>
  [HttpGet("revenue-by-category")]
  [RequiresAnyPermissions(
    Permissions.Admin.DashboardManagement.View,
    Permissions.Accountant.DashboardManagement.View)]
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
  /// Lấy doanh thu theo ngày và danh mục (multi-series cho biểu đồ).
  /// </summary>
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
