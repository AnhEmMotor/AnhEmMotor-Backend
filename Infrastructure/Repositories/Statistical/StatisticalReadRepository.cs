using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.InventoryReceipt;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalReadRepository(ApplicationDBContext context) : IStatisticalReadRepository
{
    public Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken)
    {
        return context.OutputOrders
            .IgnoreQueryFilters()
            .Where(o => string.Compare(o.StatusId, OrderStatus.Cancelled) != 0 && o.CreatedAt != null)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .Select(
                o => new RecentOrderResponse
                {
                    Id = o.Id,
                    OrderCode = $"HD{o.Id}",
                    BuyerName = o.CustomerName ?? (o.Buyer != null ? o.Buyer.FullName : "Khách lẻ"),
                    TotalAmount =
                        o.OutputInfos.Where(oi => oi.DeletedAt == null).Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                    StatusId = o.StatusId,
                    CreatedAt = o.CreatedAt!.Value
                })
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopProductRevenueResponse>> GetTopProductsByRevenueAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .Select(x => new { x.oi.ProductVariantId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var grouped = rawData.GroupBy(x => x.ProductVariantId)
            .Select(
                g => new { VariantId = g.Key, Revenue = g.Sum(x => x.Price * x.Count), SoldCount = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToList();
        var variantIds = grouped.Select(g => g.VariantId).ToList();
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return grouped.Select(
            g =>
            {
                var variant = variants.FirstOrDefault(v => v.Id == g.VariantId);
                return new TopProductRevenueResponse
                {
                    ProductName =
                        variant != null
                                ? $"{variant.Product?.Name} - {variant.VariantName} ({variant.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                                    ' ',
                                    '-',
                                    '(',
                                    ')')
                                : "Sản phẩm không xác định",
                    UnitsSold = g.SoldCount,
                    Revenue = g.Revenue
                };
            });
    }

    public async Task<IEnumerable<BrandRevenueResponse>> GetBrandRevenueDistributionAsync(
        CancellationToken cancellationToken)
    {
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .Select(x => new { x.oi.ProductVariantId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantIds = rawData.Select(x => x.ProductVariantId).Distinct().ToList();
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData.Select(
            r => new
            {
                BrandName = variants.FirstOrDefault(v => v.Id == r.ProductVariantId)?.Product?.Brand?.Name ?? "Khác",
                Revenue = r.Price * r.Count
            });
        return[.. revenueData.GroupBy(r => r.BrandName)
            .Select(g => new BrandRevenueResponse { BrandName = g.Key, Revenue = g.Sum(x => x.Revenue) })
            .OrderByDescending(b => b.Revenue)];
    }

    public async Task<IEnumerable<DailyRevenueTableResponse>> GetDailyRevenueTableDataAsync(
        int days,
        CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(days - 1)));
        var startDateTimeOffset = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dateSeries = Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset)
            .Select(
                x => new
                {
                    CreatedAt = x.o.CreatedAt!.Value,
                    OrderId = x.o.Id,
                    Price = x.oi.Price ?? 0,
                    CostPrice = x.oi.CostPrice ?? 0,
                    Count = x.oi.Count ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.DateTime))
            .Select(
                g => new
                {
                    Day = g.Key,
                    OrdersCount = g.Select(x => x.OrderId).Distinct().Count(),
                    Revenue = g.Sum(x => x.Price * x.Count),
                    Profit = g.Sum(x => (x.Price - x.CostPrice) * x.Count),
                    HasZeroCostPrice = g.Any(x => x.CostPrice == 0)
                })
            .ToList();
        var result = new List<DailyRevenueTableResponse>();
        for (int i = 0; i < dateSeries.Count; i++)
        {
            var day = dateSeries[i];
            var dayData = revenueData.FirstOrDefault(r => r.Day == day);
            var prevDayData = i > 0 ? revenueData.FirstOrDefault(r => r.Day == dateSeries[i - 1]) : null;
            double growth = 0;
            if (prevDayData != null && prevDayData.Revenue > 0 && dayData != null)
            {
                growth = (double)((dayData.Revenue - prevDayData.Revenue) / prevDayData.Revenue * 100);
            }
            result.Add(
                new DailyRevenueTableResponse
                {
                    ReportDay = day,
                    OrdersCount = dayData?.OrdersCount ?? 0,
                    TotalRevenue = dayData?.Revenue ?? 0,
                    TotalProfit = dayData?.Profit ?? 0,
                    Growth = Math.Round(growth, 2),
                    HasZeroCostPrice = dayData?.HasZeroCostPrice ?? false
                });
        }
        return result.OrderByDescending(r => r.ReportDay);
    }

    public async Task<IEnumerable<DailyRevenueDetailResponse>> GetDailyRevenueDetailAsync(
    DateOnly reportDay,
    int days,
    CancellationToken cancellationToken)
{
    var dayStart = new DateTimeOffset(reportDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    var dayEnd = dayStart.AddDays(1).AddTicks(-1);

    var rawData = await context.OutputInfos
        .IgnoreQueryFilters()
        .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
        .Where(x =>
            x.o.CreatedAt >= dayStart &&
            x.o.CreatedAt <= dayEnd &&
            (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
             string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
             string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
        .Select(x => new
        {
            x.oi.ProductVarientId,
            x.o.CreatedBy,
            Price = x.oi.Price ?? 0,
            Count = x.oi.Count ?? 0
        })
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

    var variantIds = rawData.Select(x => x.ProductVarientId).Distinct().ToList();
    var userIds = rawData.Select(x => x.CreatedBy).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

    var variants = await context.ProductVariants
        .IgnoreQueryFilters()
        .Include(pv => pv.Product)
        .Where(pv => variantIds.Contains(pv.Id))
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

    var users = await context.Users
        .IgnoreQueryFilters()
        .Where(u => userIds.Contains(u.Id))
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

    return rawData
        .GroupBy(x => new { x.ProductVarientId, CreatedBy = x.CreatedBy ?? Guid.Empty })
        .Select(g =>
        {
            var variant = variants.FirstOrDefault(v => v.Id == g.Key.ProductVarientId);
            var user = users.FirstOrDefault(u => u.Id == g.Key.CreatedBy);
            return new DailyRevenueDetailResponse
            {
                ProductName = variant != null
                    ? $"{variant.Product?.Name} - {variant.VariantName}".Trim(' ', '-')
                    : "Sản phẩm không xác định",
                EmployeeName = user?.FullName ?? "Không rõ",
                Quantity = g.Sum(x => x.Count),
                Revenue = g.Sum(x => x.Price * x.Count)
            };
        })
        .OrderByDescending(x => x.Revenue)
        .ToList();
}

public async Task<IEnumerable<DailyRevenueResponse>> GetDailyRevenueAsync(
        int days,
        CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(days - 1)));
        var startDateTimeOffset = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dateSeries = Enumerable.Range(0, days).Select(i => startDate.AddDays(i)).ToList();
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset)
            .Select(x => new { CreatedAt = x.o.CreatedAt!.Value, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.DateTime))
            .Select(g => new { Day = g.Key, Revenue = g.Sum(x => x.Price * x.Count) })
            .ToList();
        return dateSeries.Select(
            day => new DailyRevenueResponse
            {
                ReportDay = day,
                TotalRevenue = revenueData.FirstOrDefault(r => r.Day == day)?.Revenue ?? 0
            });
    }

    public async Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var yesterdayStart = todayStart.AddDays(-1);
        var currentMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);
        async Task<(decimal Rev, decimal Prof)> GetStatsInRange(DateTimeOffset start, DateTimeOffset end)
        {
            var stats = await context.OutputInfos
                .IgnoreQueryFilters()
                .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.o.CreatedAt >= start &&
                        x.o.CreatedAt <= end &&
                        (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
                .Select(
                    x => new
                    {
                        Revenue = (x.oi.Price ?? 0) * (x.oi.Count ?? 0),
                        Profit = ((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0)
                    })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return (stats.Sum(x => x.Revenue), stats.Sum(x => x.Profit));
        }

        var (Rev, Prof) = await GetStatsInRange(todayStart, now).ConfigureAwait(false);
        var yesterdayStats = await GetStatsInRange(yesterdayStart, todayStart.AddTicks(-1)).ConfigureAwait(false);
        var monthStats = await GetStatsInRange(currentMonthStart, now).ConfigureAwait(false);
        var lastMonthStats = await GetStatsInRange(lastMonthStart, lastMonthEnd).ConfigureAwait(false);
        decimal revenueChange = 0;
        if (yesterdayStats.Rev > 0)
            revenueChange = ((Rev - yesterdayStats.Rev) / yesterdayStats.Rev) * 100;
        else if (Rev > 0)
            revenueChange = 100;
        var twoHoursAgo = now.AddHours(-2);
        var overdueOrdersCount = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => (string.Compare(o.StatusId, OrderStatus.Pending) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0) &&
                    o.CreatedAt != null &&
                    o.CreatedAt <= twoHoursAgo,
                cancellationToken)
            .ConfigureAwait(false);
        var thirtyDaysAgo = now.AddDays(-30);
        var pendingOrdersCount = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => (string.Compare(o.StatusId, OrderStatus.Pending) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0) &&
                    o.CreatedAt != null &&
                    o.CreatedAt >= thirtyDaysAgo,
                cancellationToken)
            .ConfigureAwait(false);
        async Task<int> GetVehiclesSold(DateTimeOffset start, DateTimeOffset end)
        {
            var data = await context.OutputInfos
                .IgnoreQueryFilters()
                .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Join(
                    context.ProductVariants.IgnoreQueryFilters(),
                    x => x.oi.ProductVariantId,
                    pv => pv.Id,
                    (x, pv) => new { x.oi, x.o, pv })
                .Join(
                    context.Products.IgnoreQueryFilters(),
                    x => x.pv.ProductId,
                    p => p.Id,
                    (x, p) => new { x.oi, x.o, p })
                .Join(
                    context.ProductCategories.IgnoreQueryFilters(),
                    x => x.p.CategoryId,
                    c => c.Id,
                    (x, c) => new { x.oi, x.o, c })
                .Where(
                    x => x.o.CreatedAt >= start &&
                        x.o.CreatedAt <= end &&
                        (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                            string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                        string.Compare(x.c.Name, "Xe máy") == 0)
                .Select(x => x.oi.Count ?? 0)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return data.Sum();
        }

        var todayVehicles = await GetVehiclesSold(todayStart, now).ConfigureAwait(false);
        var monthVehicles = await GetVehiclesSold(currentMonthStart, now).ConfigureAwait(false);
        var confirmedInventoryReceipts = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0)
            .GroupBy(x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputs = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var totalInventory = confirmedInventoryReceipts.Sum(i => i.TotalIn) - soldOutputs.Sum(o => o.TotalOut);
        var brandData = await context.Products
            .IgnoreQueryFilters()
            .Join(context.Brands.IgnoreQueryFilters(), p => p.BrandId, b => b.Id, (p, b) => new { p, b })
            .Join(
                context.ProductVariants.IgnoreQueryFilters(),
                x => x.p.Id,
                pv => pv.ProductId,
                (x, pv) => new { x.p, x.b, pv })
            .Select(x => new { x.b.Name, VariantId = x.pv.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var brandStock = brandData
            .Select(
                x => new BrandStockResponse
                {
                    BrandName = x.Name,
                    StockCount =
                        (int)((confirmedInventoryReceipts.FirstOrDefault(i => i.VariantId == x.VariantId)?.TotalIn ?? 0) -
                                (soldOutputs.FirstOrDefault(o => o.VariantId == x.VariantId)?.TotalOut ?? 0))
                })
            .GroupBy(x => x.BrandName)
            .Select(g => new BrandStockResponse { BrandName = g.Key, StockCount = g.Sum(x => x.StockCount) })
            .Where(x => x.StockCount > 0)
            .OrderByDescending(x => x.StockCount)
            .Take(5)
            .ToList();
        int lowStockCount = 0;
        foreach (var InventoryReceipt in confirmedInventoryReceipts)
        {
            var sold = soldOutputs.FirstOrDefault(x => x.VariantId == InventoryReceipt.VariantId)?.TotalOut ?? 0;
            if (InventoryReceipt.TotalIn - sold < 3)
                lowStockCount++;
        }
        var sixtyDaysAgo = now.AddDays(-60);
        var oldInventoryReceiptVariants = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 && x.i.CreatedAt <= sixtyDaysAgo)
            .Select(x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Distinct()
            .ToList();
        int overstockCount = 0;
        foreach (var vId in oldInventoryReceiptVariants)
        {
            var tin = confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == vId)?.TotalIn ?? 0;
            var tout = soldOutputs.FirstOrDefault(x => x.VariantId == vId)?.TotalOut ?? 0;
            if (tin > tout)
                overstockCount++;
        }
        var last7DaysStart = todayStart.AddDays(-6);
        var last7DaysData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .GroupBy(x => new { x.o.CreatedAt!.Value.Year, x.o.CreatedAt!.Value.Month, x.o.CreatedAt!.Value.Day })
            .Select(
                g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                    Profit = g.Sum(x => ((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0))
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        decimal total7dRev = last7DaysData.Sum(x => x.Revenue);
        decimal total7dProf = last7DaysData.Sum(x => x.Profit);
        var bestDay = last7DaysData.OrderByDescending(x => x.Revenue).FirstOrDefault();
        var totalSKU = await context.Products.IgnoreQueryFilters().CountAsync(cancellationToken).ConfigureAwait(false);
        var activeInstallments = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0 ||
                    string.Compare(o.StatusId, OrderStatus.DepositPaid) == 0,
                cancellationToken)
            .ConfigureAwait(false);
        var topProducts = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Join(context.ProductVariants, x => x.oi.ProductVariantId, pv => pv.Id, (x, pv) => new { x.oi, x.o, pv })
            .Join(context.Products, x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
            .GroupBy(x => x.p.Name)
            .Select(
                g => new TopSellingProductResponse
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.oi.Count ?? 0),
                    TotalRevenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0))
                })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var brandRevenue = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .Join(
                context.ProductVariants.IgnoreQueryFilters(),
                x => x.oi.ProductVariantId,
                pv => pv.Id,
                (x, pv) => new { x.oi, x.o, pv })
            .Join(context.Products.IgnoreQueryFilters(), x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
            .Join(context.Brands.IgnoreQueryFilters(), x => x.p.BrandId, b => b.Id, (x, b) => new { x.oi, x.o, b })
            .GroupBy(x => x.b.Name)
            .Select(
                g => new BrandRevenueResponse
                {
                    BrandName = g.Key,
                    TotalRevenue = g.Sum(x => (x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                    QuantitySold = g.Sum(x => x.oi.Count ?? 0)
                })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var todayActivities = new List<string>();
        if (todayVehicles > 0)
            todayActivities.Add($"{todayVehicles} xe đã giao");
        var todayInst = await context.OutputOrders
            .IgnoreQueryFilters()
            .CountAsync(
                o => o.CreatedAt >= todayStart &&
                    (string.Compare(o.StatusId, OrderStatus.WaitingDeposit) == 0 ||
                        string.Compare(o.StatusId, OrderStatus.DepositPaid) == 0),
                cancellationToken)
            .ConfigureAwait(false);
        if (todayInst > 0)
            todayActivities.Add($"{todayInst} đơn trả góp mới");
        var todayCust = await context.Users
            .IgnoreQueryFilters()
            .CountAsync(u => u.CreatedAt >= todayStart, cancellationToken)
            .ConfigureAwait(false);
        if (todayCust > 0)
            todayActivities.Add($"{todayCust} khách ghé thăm");
        return new DashboardStatsResponse
        {
            TodayRevenue = Rev,
            RevenueChangePercentage = revenueChange,
            MonthlyRevenue = monthStats.Rev,
            TodayProfit = Prof,
            MonthlyProfit = monthStats.Prof,
            LastMonthRevenue = lastMonthStats.Rev,
            LastMonthProfit = lastMonthStats.Prof,
            Total7dRevenue = total7dRev,
            Total7dProfit = total7dProf,
            BestDayRevenue = bestDay?.Revenue ?? 0,
            BestDayDate = bestDay != null ? $"{bestDay.Date.Day:D2}/{bestDay.Date.Month:D2}" : null,
            OverdueOrdersCount = overdueOrdersCount,
            LowStockCount = lowStockCount,
            TodayVehiclesSold = todayVehicles,
            MonthlyVehiclesSold = monthVehicles,
            CurrentInventoryCount = (int)totalInventory,
            TotalSKUCount = totalSKU,
            OverstockCount = overstockCount,
            BrandDistribution = brandStock,
            ActiveInstallmentCount = activeInstallments,
            LateInstallmentCount = (int)(activeInstallments * 0.1),
            TotalCustomerDebt = 0,
            OverdueDebtAmount = 0,
            PendingOrdersCount = pendingOrdersCount,
            NewCustomersCount = todayCust,
            TopSellingProducts = topProducts,
            BrandRevenueDistribution = brandRevenue,
            TodayActivities = todayActivities
        };
    }

    public async Task<IEnumerable<MonthlyRevenueProfitResponse>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken)
    {
        var currentMonth = new DateOnly(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1);
        var startMonth = currentMonth.AddMonths(-(months - 1));
        var startDateTimeOffset = new DateTimeOffset(startMonth.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var monthSeries = Enumerable.Range(0, months).Select(i => startMonth.AddMonths(i)).ToList();
        var rawData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt != null &&
                    x.o.CreatedAt >= startDateTimeOffset)
            .Select(
                x => new
                {
                    CreatedAt = x.o.CreatedAt!.Value,
                    Price = x.oi.Price ?? 0,
                    CostPrice = x.oi.CostPrice ?? 0,
                    Count = x.oi.Count ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData
            .GroupBy(x => new DateOnly(x.CreatedAt.DateTime.Year, x.CreatedAt.DateTime.Month, 1))
            .Select(
                g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.Price * x.Count),
                    Profit = g.Sum(x => (x.Price - x.CostPrice) * x.Count),
                    HasZeroCostPrice = g.Any(x => x.CostPrice == 0)
                })
            .ToList();
        return monthSeries.Select(
            month => new MonthlyRevenueProfitResponse
            {
                ReportMonth = month,
                TotalRevenue = revenueData.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0,
                TotalProfit = revenueData.FirstOrDefault(r => r.Month == month)?.Profit ?? 0,
                HasZeroCostPrice = revenueData.FirstOrDefault(r => r.Month == month)?.HasZeroCostPrice ?? false
            });
    }

    public Task<IEnumerable<OrderStatusCountResponse>> GetOrderStatusCountsAsync(CancellationToken cancellationToken)
    {
        return context.OutputStatuses
            .IgnoreQueryFilters()
            .GroupJoin(
                context.OutputOrders.IgnoreQueryFilters(),
                os => os.Key,
                o => o.StatusId,
                (os, orders) => new OrderStatusCountResponse { StatusName = os.Key, OrderCount = orders.Count() })
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<OrderStatusCountResponse>>(t => t.Result, cancellationToken);
    }

    public async Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(
        CancellationToken cancellationToken)
    {
        var lastMonthStart = new DateTimeOffset(
            DateTimeOffset.UtcNow.AddMonths(-1).Year,
            DateTimeOffset.UtcNow.AddMonths(-1).Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);
        var currentMonthStart = new DateTimeOffset(
            DateTimeOffset.UtcNow.Year,
            DateTimeOffset.UtcNow.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);
        var confirmedInventoryReceipts = await context.InventoryReceiptInfos
            .IgnoreQueryFilters()
            .Join(
                context.InventoryReceipts.IgnoreQueryFilters(),
                ii => ii.InventoryReceiptId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0)
            .GroupBy(x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldLastMonth = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                        string.Compare(x.o.StatusId, OrderStatus.Completed) == 0) &&
                    x.o.CreatedAt >= lastMonthStart &&
                    x.o.CreatedAt < currentMonthStart)
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return variants.Select(
            pv => new ProductReportResponse
            {
                ProductName =
                    $"{pv.Product?.Name} - {pv.VariantName} ({pv.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                            ' ',
                            '-',
                            '(',
                            ')'),
                VariantId = pv.Id,
                StockQuantity =
                    (int)((confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                            (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0)),
                SoldLastMonth = (int)(soldLastMonth.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0)
            });
    }

    public async Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(
        CancellationToken cancellationToken)
    {
        var last30Days = new DateTimeOffset(DateTime.UtcNow.AddDays(-30), TimeSpan.Zero);
        var confirmedInventoryReceipts = await context.InventoryReceiptInfos
            .Join(context.InventoryReceipts, ii => ii.InventoryReceiptId, i => i.Id, (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.DeletedAt == null &&
                    x.i.DeletedAt == null)
            .GroupBy(x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var outputsData = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0)
            .Select(
                x => new
                {
                    x.oi.ProductVariantId,
                    x.o.CreatedAt,
                    Count = x.oi.Count ?? 0,
                    Price = x.oi.Price ?? 0,
                    Cost = x.oi.CostPrice ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = outputsData
            .GroupBy(x => x.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)x.Count) })
            .ToList();
        var soldLast30Days = outputsData
            .Where(x => x.CreatedAt >= last30Days)
            .GroupBy(x => x.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)x.Count) })
            .ToList();
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .Include(pv => pv.ProductVariantColors)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return[.. variants.Select(
            pv =>
            {
                var stock = (int)((confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                    (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0));
                var sold30 = (int)(soldLast30Days.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0);
                var variantOutputs = outputsData.Where(x => x.ProductVariantId == pv.Id).ToList();
                var totalRevenue = variantOutputs.Sum(x => x.Price * x.Count);
                var totalCost = variantOutputs.Sum(x => x.Cost * x.Count);
                var margin = totalRevenue > 0 ? (double)((totalRevenue - totalCost) / totalRevenue * 100) : 0;
                var sellPrice = pv.Price ?? 0;
                return new ProductPerformanceTableResponse
                {
                    ProductName =
                        $"{pv.Product?.Name} - {pv.VariantName} ({pv.ProductVariantColors.FirstOrDefault()?.ColorName?.Split(',').FirstOrDefault()})".Trim(
                                ' ',
                                '-',
                                '(',
                                ')'),
                    SellPrice = sellPrice,
                    SoldCount30Days = sold30,
                    StockQuantity = stock,
                    MaxStockQuantity = 100,
                    MarginPercentage = Math.Round(margin, 1),
                    Status = stock <= 0 ? "Hết hàng" : (stock < 5 ? "Sắp hết" : "Còn hàng"),
                    Trend = [0, 0, sold30]
                };
            })];
    }

    public async Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(
        CancellationToken cancellationToken)
    {
        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInventoryReceipts = await context.InventoryReceiptInfos
            .Join(context.InventoryReceipts, ii => ii.InventoryReceiptId, i => i.Id, (ii, i) => new { ii, i })
            .Where(
                x => string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0 &&
                    x.ii.DeletedAt == null &&
                    x.i.DeletedAt == null)
            .GroupBy(x => (x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null))
            .Select(
                g => new
                {
                    VariantId = g.Key,
                    TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)),
                    AvgInventoryReceiptPrice = g.Sum(
                                x => x.ii.PurchaseRequestItem != null ? (x.ii.PurchaseRequestItem.UnitPrice ?? 0) : 0 *
                                    (x.ii.Count ?? 0)) /
                        (g.Sum(x => (long)(x.ii.Count ?? 0)) == 0 ? 1M : (decimal)(g.Sum(x => (long)(x.ii.Count ?? 0))))
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = await context.OutputInfos
            .IgnoreQueryFilters()
            .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                    string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
            .GroupBy(x => x.oi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantDatas = variants.Select(
            pv =>
            {
                var confirmedInventoryReceipt = confirmedInventoryReceipts.FirstOrDefault(x => x.VariantId == pv.Id);
                var stock = (int)((confirmedInventoryReceipt?.TotalIn ?? 0) -
                    (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0));
                var costPrice = confirmedInventoryReceipt?.AvgInventoryReceiptPrice ?? 0;
                return new { BrandName = pv.Product?.Brand?.Name, Stock = stock, Value = stock * costPrice };
            });
        var grouped = variantDatas
            .GroupBy(x => x.BrandName ?? "Khác")
            .Select(
                g =>
                {
                    int totalStock = g.Sum(x => x.Stock);
                    int lowStock = g.Count(x => x.Stock > 0 && x.Stock < 5);
                    int outOfStock = g.Count(x => x.Stock <= 0);
                    decimal value = g.Sum(x => x.Value);
                    return new WarehouseTableDataResponse
                    {
                        BrandName = g.Key,
                        TotalStock = totalStock,
                        Capacity = totalStock > 0 ? (totalStock + 100) : 100,
                        LowStock = lowStock,
                        OutOfStock = outOfStock,
                        Status = outOfStock > 0 ? "Cảnh báo" : "Bình thường",
                        Value = value
                    };
                })
            .OrderByDescending(x => x.TotalStock)
            .ToList();
        return grouped;
    }

    public async Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == variantId, cancellationToken)
            .ConfigureAwait(false);
        if (variant is null)
        {
            return null;
        }
        var totalInventoryReceipt = await context.InventoryReceiptInfos
                .IgnoreQueryFilters()
                .Join(
                    context.InventoryReceipts.IgnoreQueryFilters(),
                    ii => ii.InventoryReceiptId,
                    i => i.Id,
                    (ii, i) => new { ii, i })
                .Where(
                    x => ((x.ii.PurchaseRequestItem != null ? x.ii.PurchaseRequestItem.ProductVariantId : (int?)null)) ==
                            variantId &&
                            string.Compare(x.i.StatusId, InventoryReceiptStatus.Approve) == 0)
                .SumAsync(x => (long?)(x.ii.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        var totalOutput = await context.OutputInfos
                .IgnoreQueryFilters()
                .Join(context.OutputOrders.IgnoreQueryFilters(), oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.oi.ProductVariantId == variantId &&
                            (string.Compare(x.o.StatusId, OrderStatus.Delivering) == 0 ||
                                string.Compare(x.o.StatusId, OrderStatus.WaitingPickup) == 0 ||
                                string.Compare(x.o.StatusId, OrderStatus.Completed) == 0))
                .SumAsync(x => (long?)(x.oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        return new ProductStockPriceResponse
        {
            UnitPrice = variant.Price ?? 0,
            StockQuantity = (int)totalInventoryReceipt - (int)totalOutput
        };
    }

    private async Task<List<ConfirmedInputSummary>> GetConfirmedInputSummariesAsync(
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        if (string.Compare(context.Database.ProviderName, "Microsoft.EntityFrameworkCore.SqlServer") != 0)
        {
            var query = includeDeleted
                ? context.InputInfos
                    .IgnoreQueryFilters()
                    .Join(context.InputReceipts.IgnoreQueryFilters(), ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
                : context.InputInfos
                    .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i });

            return await query
                .Where(x => string.Compare(x.i.StatusId, InputStatus.Finish) == 0)
                .GroupBy(x => x.ii.ProductId)
                .Select(
                    g => new ConfirmedInputSummary(
                        g.Key,
                        g.Sum(x => (long)(x.ii.Count ?? 0)),
                        g.Sum(x => (x.ii.InputPrice ?? 0) * (x.ii.Count ?? 0)),
                        g.Min(x => x.i.CreatedAt)))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        if (await TableExistsAsync("InventoryReceiptInfo", cancellationToken).ConfigureAwait(false) &&
            await TableExistsAsync("InventoryReceipt", cancellationToken).ConfigureAwait(false))
        {
            return await ReadConfirmedInputSummariesAsync(
                    """
                    SELECT
                        [Info].[ProductVariantId] AS [VariantId],
                        SUM(CAST(ISNULL([Info].[Count], 0) AS bigint)) AS [TotalIn],
                        CAST(0 AS decimal(18, 2)) AS [WeightedInputTotal],
                        MIN([Receipt].[CreatedAt]) AS [FirstCreatedAt]
                    FROM [dbo].[InventoryReceiptInfo] AS [Info]
                    INNER JOIN [dbo].[InventoryReceipt] AS [Receipt]
                        ON [Info].[InventoryReceiptId] = [Receipt].[Id]
                    WHERE [Info].[ProductVariantId] IS NOT NULL
                      AND [Receipt].[StatusId] = @FinishedStatus
                      AND (@IncludeDeleted = 1 OR ([Info].[DeletedAt] IS NULL AND [Receipt].[DeletedAt] IS NULL))
                    GROUP BY [Info].[ProductVariantId]
                    """,
                    includeDeleted,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (await TableExistsAsync("InputInfo", cancellationToken).ConfigureAwait(false) &&
            await TableExistsAsync("Input", cancellationToken).ConfigureAwait(false))
        {
            return await ReadConfirmedInputSummariesAsync(
                    """
                    SELECT
                        [Info].[ProductId] AS [VariantId],
                        SUM(CAST(ISNULL([Info].[Count], 0) AS bigint)) AS [TotalIn],
                        SUM(CAST(ISNULL([Info].[InputPrice], 0) AS decimal(18, 2)) * CAST(ISNULL([Info].[Count], 0) AS decimal(18, 2))) AS [WeightedInputTotal],
                        MIN([Receipt].[CreatedAt]) AS [FirstCreatedAt]
                    FROM [dbo].[InputInfo] AS [Info]
                    INNER JOIN [dbo].[Input] AS [Receipt]
                        ON [Info].[InputId] = [Receipt].[Id]
                    WHERE [Info].[ProductId] IS NOT NULL
                      AND [Receipt].[StatusId] = @FinishedStatus
                      AND (@IncludeDeleted = 1 OR ([Info].[DeletedAt] IS NULL AND [Receipt].[DeletedAt] IS NULL))
                    GROUP BY [Info].[ProductId]
                    """,
                    includeDeleted,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return [];
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State is not ConnectionState.Open;
        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT CASE WHEN OBJECT_ID(@TableName, N'U') IS NULL THEN 0 ELSE 1 END";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@TableName";
            parameter.Value = $"[dbo].[{tableName}]";
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return result is int value && value == 1;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task<List<ConfirmedInputSummary>> ReadConfirmedInputSummariesAsync(
        string commandText,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var results = new List<ConfirmedInputSummary>();
        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State is not ConnectionState.Open;
        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.Parameters.Add(CreateParameter(command, "@FinishedStatus", InputStatus.Finish));
            command.Parameters.Add(CreateParameter(command, "@IncludeDeleted", includeDeleted ? 1 : 0));
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(
                    new ConfirmedInputSummary(
                        reader.IsDBNull(0) ? null : reader.GetInt32(0),
                        reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                        reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                        reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)));
            }
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        return results;
    }

    private static DbParameter CreateParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;

        return parameter;
    }

    private sealed record ConfirmedInputSummary(
        int? VariantId,
        long TotalIn,
        decimal WeightedInputTotal,
        DateTimeOffset? FirstCreatedAt)
    {
        public decimal AverageInputPrice => TotalIn == 0 ? 0 : WeightedInputTotal / TotalIn;
    }

    public async Task<WorkshopOverviewResponse> GetWorkshopOverviewAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var allOrders = await context.RepairOrders
            .IgnoreQueryFilters()
            .Include(ro => ro.Technician)
            .ThenInclude(t => t!.User)
            .Include(ro => ro.Vehicle)
            .ThenInclude(v => v!.Product)
            .ToListAsync(cancellationToken);

        var inProgressCount = allOrders.Count(ro => ro.Status == "InProgress");

        var completedOrders = allOrders.Where(ro => ro.Status == "Completed" && ro.StartTime != null && ro.CompletedDate != null).ToList();
        double avgCompletionHours = 0;
        if (completedOrders.Count > 0)
        {
            avgCompletionHours = completedOrders.Average(ro => (ro.CompletedDate.Value - ro.StartTime.Value).TotalHours);
        }

        var monthlyRevenue = allOrders
            .Where(ro => ro.Status == "Completed" && ro.CompletedDate >= startOfMonth)
            .Sum(ro => ro.TotalAmount);

        var overdueCount = allOrders.Count(ro => ro.Status != "Completed" && ro.ExpectedCompletionTime < now);

        var activeRepairOrders = allOrders
            .Where(ro => ro.Status == "InProgress" || ro.Status == "Pending")
            .OrderByDescending(ro => ro.CreatedAt)
            .Select(ro => {
                string statusVN = ro.Status == "InProgress" ? "Đang sửa" : "Chờ phụ tùng";
                string techName = ro.Technician?.User?.FullName ?? "Chưa phân công";
                string vehicleName = ro.Vehicle?.Product?.Name ?? "Xe máy khách hàng";
                if (ro.Vehicle != null && !string.IsNullOrEmpty(ro.Vehicle.LicensePlate))
                {
                    vehicleName += $" ({ro.Vehicle.LicensePlate})";
                }
                return new WorkshopRepairOrderDto
                {
                    Id = ro.Id,
                    OrderCode = $"SC{ro.Id}",
                    CustomerName = ro.CustomerName,
                    VehicleInfo = vehicleName,
                    TechnicianName = techName,
                    Status = statusVN,
                    StartedAt = ro.StartTime,
                    LaborFee = ro.LaborCost
                };
            })
            .ToList();

        return new WorkshopOverviewResponse
        {
            Kpi = new WorkshopKpi
            {
                InProgressCount = inProgressCount,
                AvgCompletionHours = Math.Round(avgCompletionHours, 1),
                MonthlyRevenue = monthlyRevenue,
                OverdueCount = overdueCount
            },
            RepairOrders = activeRepairOrders
        };
    }

    public async Task<FinancingOverviewResponse> GetFinancingOverviewAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var contracts = await context.FinanceContracts
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var totalApplications = contracts.Count;
        var disbursedCount = contracts.Count(c => c.DisbursementStatus == "Disbursed");
        var pendingCount = contracts.Count(c => c.DisbursementStatus == "Pending");
        var overdueCount = contracts.Count(c => c.DisbursementStatus == "Pending" && c.SignedDate < now);

        var list = contracts.Select(c => {
            string statusVN = c.DisbursementStatus == "Disbursed" ? "Đã giải ngân" : (c.SignedDate < now ? "Chờ giải ngân" : "Chờ duyệt");

            string cavetVN = c.CavetLocation switch
            {
                "Bank" => "Công ty tài chính giữ",
                "Store" => "Cửa hàng giữ hộ",
                "Customer" => "Đã giao khách",
                _ => "Chưa xác định"
            };

            var cust360 = Application.Features.FinanceContracts.FinanceContractCustomer360Catalog.GetCustomer360(c.ContractNumber);
            string customerName = cust360?.FullName ?? "Khách hàng trả góp";

            string vehicleName = c.ContractNumber switch
            {
                string s when s.Contains("HDSAISON") => "Honda Vision 110cc",
                string s when s.Contains("FECREDIT") => "Honda SH 150i",
                string s when s.Contains("HOMECREDIT") => "Yamaha Exciter 155",
                string s when s.Contains("MBANK") => "Suzuki Raider R150",
                _ => "Honda Vision 110cc"
            };

            return new FinancingInstallmentDto
            {
                ApplicationCode = c.ContractNumber,
                CustomerName = customerName,
                PartnerName = c.BankName,
                VehicleName = vehicleName,
                Amount = c.LoanAmount,
                Status = statusVN,
                CavetStatus = cavetVN,
                CreatedAt = c.CreatedAt
            };
        }).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            list[i].Id = i + 1;
        }

        return new FinancingOverviewResponse
        {
            Kpi = new FinancingKpi
            {
                TotalApplications = totalApplications,
                DisbursedCount = disbursedCount,
                PendingCount = pendingCount,
                OverdueCount = overdueCount
            },
            Installments = list
        };
    }

    public async Task<CustomerAnalyticsResponse> GetCustomerAnalyticsAsync(CancellationToken cancellationToken)
    {
        var leads = await context.Leads
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
        var orders = await context.OutputOrders
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var totalLeads = leads.Count;
        var hotLeads = leads.Count(l => l.Score >= 80);
        var newCustomers = orders.Select(o => o.CustomerPhone).Distinct().Count();

        var leadList = leads
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new CustomerLeadDto
            {
                Id = l.Id,
                CustomerName = l.FullName,
                Source = l.Source switch
                {
                    "WebStore" => "Website",
                    "Facebook" => "Facebook",
                    "Shop" => "Showroom",
                    _ => l.Source
                },
                LeadScore = l.Score,
                Status = l.Status switch
                {
                    "New" => "Mới",
                    "Consulting" => "Đang theo dõi",
                    "Converted" => "Đã chuyển đổi",
                    "Lost" => "Không quan tâm",
                    "TestDriving" => "Đang theo dõi",
                    _ => l.Status
                },
                LastContact = l.UpdatedAt ?? l.CreatedAt
            })
            .ToList();

        return new CustomerAnalyticsResponse
        {
            Kpi = new CustomerKpi
            {
                TotalLeads = totalLeads,
                NewCustomers = newCustomers,
                HotLeads = hotLeads
            },
            Leads = leadList
        };
    }

    public async Task<CustomerServiceAnalyticsResponse> GetCustomerServiceAnalyticsAsync(CancellationToken cancellationToken)
    {
        var contacts = await context.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Replies)
            .ToListAsync(cancellationToken);

        var totalWithRating = contacts.Where(c => c.Rating != null).ToList();
        double avgRating = totalWithRating.Count > 0 ? totalWithRating.Average(c => c.Rating.Value) : 5.0;

        var newComplaints = contacts.Count(c => c.Status == "Pending");
        var resolvedCount = contacts.Count(c => c.Status == "Closed");

        double avgResponseHours = 0;
        var repliedContacts = contacts.Where(c => c.Replies != null && c.Replies.Any(r => r.CreatedAt != null) && c.CreatedAt != null).ToList();
        if (repliedContacts.Count > 0)
        {
            avgResponseHours = repliedContacts.Average(c => {
                var firstReply = c.Replies.Where(r => r.CreatedAt != null).OrderBy(r => r.CreatedAt).First();
                return (firstReply.CreatedAt!.Value - c.CreatedAt!.Value).TotalHours;
            });
        }

        var complaintList = contacts
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => {
                double? respHours = null;
                if (c.Replies != null && c.Replies.Any(r => r.CreatedAt != null) && c.CreatedAt != null)
                {
                    var firstReply = c.Replies.Where(r => r.CreatedAt != null).OrderBy(r => r.CreatedAt).First();
                    respHours = Math.Round((firstReply.CreatedAt!.Value - c.CreatedAt!.Value).TotalHours, 1);
                }

                string statusVN = c.Status switch
                {
                    "Pending" => "Mới",
                    "Replied" => "Đã phản hồi",
                    "Closed" => "Đã đóng",
                    _ => c.Status
                };

                return new CustomerComplaintDto
                {
                    Id = c.Id,
                    TicketCode = $"LH{c.Id}",
                    CustomerName = c.FullName,
                    Subject = c.Subject,
                    Rating = c.Rating ?? 0,
                    Status = statusVN,
                    CreatedAt = c.CreatedAt,
                    ResponseHours = respHours
                };
            })
            .ToList();

        return new CustomerServiceAnalyticsResponse
        {
            Kpi = new CustomerServiceKpi
            {
                AvgRating = Math.Round(avgRating, 1),
                NewComplaints = newComplaints,
                AvgResponseHours = Math.Round(avgResponseHours, 1),
                ResolvedCount = resolvedCount
            },
            Complaints = complaintList
        };
    }
}

