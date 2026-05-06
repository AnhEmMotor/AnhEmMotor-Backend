using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.Input;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalReadRepository(ApplicationDBContext context) : IStatisticalReadRepository
{
    public Task<List<RecentOrderResponse>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken)
    {
        return context.OutputOrders
            .Where(o => o.StatusId != OrderStatus.Cancelled && o.CreatedAt != null)
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
                x => x.o.StatusId == OrderStatus.Delivering ||
                    x.o.StatusId == OrderStatus.WaitingPickup ||
                    x.o.StatusId == OrderStatus.Completed)
            .Select(x => new { x.oi.ProductVarientId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var grouped = rawData.GroupBy(x => x.ProductVarientId)
            .Select(
                g => new { VariantId = g.Key, Revenue = g.Sum(x => x.Price * x.Count), SoldCount = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToList();
        var variantIds = grouped.Select(g => g.VariantId).ToList();
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
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
                                ? $"{variant.Product?.Name} - {variant.VersionName} ({variant.ColorName?.Split(',').FirstOrDefault()})".Trim(
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
                x => x.o.StatusId == OrderStatus.Delivering ||
                    x.o.StatusId == OrderStatus.WaitingPickup ||
                    x.o.StatusId == OrderStatus.Completed)
            .Select(x => new { x.oi.ProductVarientId, Price = x.oi.Price ?? 0, Count = x.oi.Count ?? 0 })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantIds = rawData.Select(x => x.ProductVarientId).Distinct().ToList();
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .Where(pv => variantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var revenueData = rawData.Select(
            r => new
            {
                BrandName = variants.FirstOrDefault(v => v.Id == r.ProductVarientId)?.Product?.Brand?.Name ?? "Khác",
                Revenue = r.Price * r.Count
            });
        return revenueData.GroupBy(r => r.BrandName)
            .Select(g => new BrandRevenueResponse { BrandName = g.Key, Revenue = g.Sum(x => x.Revenue) })
            .OrderByDescending(b => b.Revenue)
            .ToList();
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
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
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
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
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
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.o.CreatedAt >= start &&
                        x.o.CreatedAt <= end &&
                        (x.o.StatusId == OrderStatus.Delivering ||
                            x.o.StatusId == OrderStatus.WaitingPickup ||
                            x.o.StatusId == OrderStatus.Completed))
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

        var todayStats = await GetStatsInRange(todayStart, now);
        var yesterdayStats = await GetStatsInRange(yesterdayStart, todayStart.AddTicks(-1));
        var monthStats = await GetStatsInRange(currentMonthStart, now);
        var lastMonthStats = await GetStatsInRange(lastMonthStart, lastMonthEnd);
        decimal revenueChange = 0;
        if (yesterdayStats.Rev > 0)
            revenueChange = ((todayStats.Rev - yesterdayStats.Rev) / yesterdayStats.Rev) * 100;
        else if (todayStats.Rev > 0)
            revenueChange = 100;
        var twoHoursAgo = now.AddHours(-2);
        var overdueOrdersCount = await context.OutputOrders
            .CountAsync(
                o => (o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit) &&
                    o.CreatedAt != null &&
                    o.CreatedAt <= twoHoursAgo,
                cancellationToken)
            .ConfigureAwait(false);
        var pendingOrdersCount = await context.OutputOrders
            .CountAsync(
                o => o.StatusId == OrderStatus.Pending || o.StatusId == OrderStatus.WaitingDeposit,
                cancellationToken)
            .ConfigureAwait(false);
        async Task<int> GetVehiclesSold(DateTimeOffset start, DateTimeOffset end)
        {
            var data = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Join(
                    context.ProductVariants,
                    x => x.oi.ProductVarientId,
                    pv => pv.Id,
                    (x, pv) => new { x.oi, x.o, pv })
                .Join(context.Products, x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
                .Join(context.ProductCategories, x => x.p.CategoryId, c => c.Id, (x, c) => new { x.oi, x.o, c })
                .Where(
                    x => x.o.CreatedAt >= start &&
                        x.o.CreatedAt <= end &&
                        (x.o.StatusId == OrderStatus.Delivering ||
                            x.o.StatusId == OrderStatus.WaitingPickup ||
                            x.o.StatusId == OrderStatus.Completed) &&
                        x.c.Name == "Xe máy")
                .Select(x => x.oi.Count ?? 0)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return data.Sum();
        }

        var todayVehicles = await GetVehiclesSold(todayStart, now);
        var monthVehicles = await GetVehiclesSold(currentMonthStart, now);
        var confirmedInputs = await context.InputInfos
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish)
            .GroupBy(x => x.ii.ProductId)
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputs = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.StatusId == OrderStatus.Delivering ||
                    x.o.StatusId == OrderStatus.WaitingPickup ||
                    x.o.StatusId == OrderStatus.Completed)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var totalInventory = confirmedInputs.Sum(i => i.TotalIn) - soldOutputs.Sum(o => o.TotalOut);
        var brandData = await context.Products
            .Join(context.Brands, p => p.BrandId, b => b.Id, (p, b) => new { p, b })
            .Join(context.ProductVariants, x => x.p.Id, pv => pv.ProductId, (x, pv) => new { x.p, x.b, pv })
            .Select(x => new { x.b.Name, VariantId = x.pv.Id })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var brandStock = brandData
            .Select(
                x => new BrandStockResponse
                {
                    BrandName = x.Name,
                    StockCount =
                        (int)((confirmedInputs.FirstOrDefault(i => i.VariantId == x.VariantId)?.TotalIn ?? 0) -
                                (soldOutputs.FirstOrDefault(o => o.VariantId == x.VariantId)?.TotalOut ?? 0))
                })
            .GroupBy(x => x.BrandName)
            .Select(g => new BrandStockResponse { BrandName = g.Key, StockCount = g.Sum(x => x.StockCount) })
            .Where(x => x.StockCount > 0)
            .OrderByDescending(x => x.StockCount)
            .Take(5)
            .ToList();
        int lowStockCount = 0;
        foreach (var input in confirmedInputs)
        {
            var sold = soldOutputs.FirstOrDefault(x => x.VariantId == input.VariantId)?.TotalOut ?? 0;
            if (input.TotalIn - sold < 3)
                lowStockCount++;
        }
        var sixtyDaysAgo = now.AddDays(-60);
        var oldInputVariants = await context.InputInfos
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish && x.i.CreatedAt <= sixtyDaysAgo)
            .Select(x => x.ii.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        int overstockCount = 0;
        foreach (var vId in oldInputVariants)
        {
            var tin = confirmedInputs.FirstOrDefault(x => x.VariantId == vId)?.TotalIn ?? 0;
            var tout = soldOutputs.FirstOrDefault(x => x.VariantId == vId)?.TotalOut ?? 0;
            if (tin > tout)
                overstockCount++;
        }
        var last7DaysStart = todayStart.AddDays(-6);
        var last7DaysData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed))
            .GroupBy(
                x => new
                {
                    Year = x.o.CreatedAt!.Value.Year,
                    Month = x.o.CreatedAt!.Value.Month,
                    Day = x.o.CreatedAt!.Value.Day
                })
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
        var totalSKU = await context.Products.CountAsync(cancellationToken).ConfigureAwait(false);
        var activeInstallments = await context.OutputOrders
            .CountAsync(
                o => o.StatusId == OrderStatus.WaitingDeposit || o.StatusId == OrderStatus.DepositPaid,
                cancellationToken)
            .ConfigureAwait(false);
        var topProducts = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed))
            .Join(context.ProductVariants, x => x.oi.ProductVarientId, pv => pv.Id, (x, pv) => new { x.oi, x.o, pv })
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
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.CreatedAt >= last7DaysStart &&
                    (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed))
            .Join(context.ProductVariants, x => x.oi.ProductVarientId, pv => pv.Id, (x, pv) => new { x.oi, x.o, pv })
            .Join(context.Products, x => x.pv.ProductId, p => p.Id, (x, p) => new { x.oi, x.o, p })
            .Join(context.Brands, x => x.p.BrandId, b => b.Id, (x, b) => new { x.oi, x.o, b })
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
            .CountAsync(
                o => o.CreatedAt >= todayStart &&
                    (o.StatusId == OrderStatus.WaitingDeposit || o.StatusId == OrderStatus.DepositPaid),
                cancellationToken)
            .ConfigureAwait(false);
        if (todayInst > 0)
            todayActivities.Add($"{todayInst} đơn trả góp mới");
        var todayCust = await context.Users
            .CountAsync(u => u.CreatedAt >= todayStart, cancellationToken)
            .ConfigureAwait(false);
        if (todayCust > 0)
            todayActivities.Add($"{todayCust} khách ghé thăm");
        return new DashboardStatsResponse
        {
            TodayRevenue = todayStats.Rev,
            RevenueChangePercentage = revenueChange,
            MonthlyRevenue = monthStats.Rev,
            TodayProfit = todayStats.Prof,
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
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
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
            .GroupJoin(
                context.OutputOrders,
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
        var confirmedInputs = await context.InputInfos
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish)
            .GroupBy(x => x.ii.ProductId)
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.StatusId == OrderStatus.Delivering ||
                    x.o.StatusId == OrderStatus.WaitingPickup ||
                    x.o.StatusId == OrderStatus.Completed)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldLastMonth = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
                    x.o.CreatedAt >= lastMonthStart &&
                    x.o.CreatedAt < currentMonthStart)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return variants.Select(
            pv => new ProductReportResponse
            {
                ProductName =
                    $"{pv.Product?.Name} - {pv.VersionName} ({pv.ColorName?.Split(',').FirstOrDefault()})".Trim(
                            ' ',
                            '-',
                            '(',
                            ')'),
                VariantId = pv.Id,
                StockQuantity =
                    (int)((confirmedInputs.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                            (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0)),
                SoldLastMonth = (int)(soldLastMonth.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0)
            });
    }

    public async Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(
        CancellationToken cancellationToken)
    {
        var last30Days = new DateTimeOffset(DateTime.UtcNow.AddDays(-30), TimeSpan.Zero);
        var confirmedInputs = await context.InputInfos
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish && x.ii.DeletedAt == null && x.i.DeletedAt == null)
            .GroupBy(x => x.ii.ProductId)
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var outputsData = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
                    x.oi.DeletedAt == null &&
                    x.o.DeletedAt == null)
            .Select(
                x => new
                {
                    x.oi.ProductVarientId,
                    x.o.CreatedAt,
                    Count = x.oi.Count ?? 0,
                    Price = x.oi.Price ?? 0,
                    Cost = x.oi.CostPrice ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = outputsData
            .GroupBy(x => x.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)x.Count) })
            .ToList();
        var soldLast30Days = outputsData
            .Where(x => x.CreatedAt >= last30Days)
            .GroupBy(x => x.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)x.Count) })
            .ToList();
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return variants.Select(
            pv =>
            {
                var stock = (int)((confirmedInputs.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                    (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0));
                var sold30 = (int)(soldLast30Days.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0);
                var variantOutputs = outputsData.Where(x => x.ProductVarientId == pv.Id).ToList();
                var totalRevenue = variantOutputs.Sum(x => x.Price * x.Count);
                var totalCost = variantOutputs.Sum(x => x.Cost * x.Count);
                var margin = totalRevenue > 0 ? (double)((totalRevenue - totalCost) / totalRevenue * 100) : 0;
                var sellPrice = pv.Price ?? 0;
                return new ProductPerformanceTableResponse
                {
                    ProductName =
                        $"{pv.Product?.Name} - {pv.VersionName} ({pv.ColorName?.Split(',').FirstOrDefault()})".Trim(
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
                    Trend = new[] { 0, 0, sold30 }
                };
            })
            .ToList();
    }

    public async Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(
        CancellationToken cancellationToken)
    {
        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .ThenInclude(p => p!.Brand)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var confirmedInputs = await context.InputInfos
            .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish && x.ii.DeletedAt == null && x.i.DeletedAt == null)
            .GroupBy(x => x.ii.ProductId)
            .Select(
                g => new
                {
                    VariantId = g.Key,
                    TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)),
                    AvgInputPrice = g.Sum(x => (x.ii.InputPrice ?? 0) * (x.ii.Count ?? 0)) /
                        (g.Sum(x => (long)(x.ii.Count ?? 0)) == 0 ? 1M : (decimal)(g.Sum(x => (long)(x.ii.Count ?? 0))))
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var soldOutputsAll = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => (x.o.StatusId == OrderStatus.Delivering ||
                        x.o.StatusId == OrderStatus.WaitingPickup ||
                        x.o.StatusId == OrderStatus.Completed) &&
                    x.oi.DeletedAt == null &&
                    x.o.DeletedAt == null)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var variantDatas = variants.Select(
            pv =>
            {
                var confirmedInput = confirmedInputs.FirstOrDefault(x => x.VariantId == pv.Id);
                var stock = (int)((confirmedInput?.TotalIn ?? 0) -
                    (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0));
                var costPrice = confirmedInput?.AvgInputPrice ?? 0;
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
        var totalInput = await context.InputInfos
                .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
                .Where(x => x.ii.ProductId == variantId && x.i.StatusId == InputStatus.Finish)
                .SumAsync(x => (long?)(x.ii.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        var totalOutput = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.oi.ProductVarientId == variantId &&
                            (x.o.StatusId == OrderStatus.Delivering ||
                                x.o.StatusId == OrderStatus.WaitingPickup ||
                                x.o.StatusId == OrderStatus.Completed))
                .SumAsync(x => (long?)(x.oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;
        return new ProductStockPriceResponse
        {
            UnitPrice = variant.Price ?? 0,
            StockQuantity = (int)totalInput - (int)totalOutput
        };
    }
}
