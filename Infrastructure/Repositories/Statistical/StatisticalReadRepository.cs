using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalReadRepository(ApplicationDBContext context) : IStatisticalReadRepository
{
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
                x => x.o.StatusId != OrderStatus.Cancelled &&
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

        var lastMonthRevenue = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.o.CreatedAt >= lastMonthStart &&
                            x.o.CreatedAt < currentMonthStart &&
                            x.o.StatusId != OrderStatus.Cancelled)
                .SumAsync(x => (long?)(x.oi.Price ?? 0) * (x.oi.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;

        var lastMonthProfit = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(
                    x => x.o.CreatedAt >= lastMonthStart &&
                            x.o.CreatedAt < currentMonthStart &&
                            x.o.StatusId != OrderStatus.Cancelled)
                .SumAsync(
                    x => (long?)((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0),
                    cancellationToken)
                .ConfigureAwait(false) ??
            0;

        var pendingOrdersCount = await context.OutputOrders
            .Where(o => o.StatusId == OrderStatus.Pending)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new DashboardStatsResponse
        {
            LastMonthRevenue = lastMonthRevenue,
            LastMonthProfit = lastMonthProfit,
            PendingOrdersCount = pendingOrdersCount,
            NewCustomersCount = 0
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
                x => x.o.StatusId != OrderStatus.Cancelled &&
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
                    Profit = g.Sum(x => (x.Price - x.CostPrice) * x.Count)
                })
            .ToList();

        return monthSeries.Select(
            month => new MonthlyRevenueProfitResponse
            {
                ReportMonth = month,
                TotalRevenue = revenueData.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0,
                TotalProfit = revenueData.FirstOrDefault(r => r.Month == month)?.Profit ?? 0
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
            .Where(x => x.i.StatusId == Domain.Constants.Input.InputStatus.Finish)
            .GroupBy(x => x.ii.ProductId)
            .Select(g => new { VariantId = g.Key, TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soldOutputsAll = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soldLastMonth = await context.OutputInfos
            .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
            .Where(
                x => x.o.StatusId != OrderStatus.Cancelled &&
                    x.o.CreatedAt >= lastMonthStart &&
                    x.o.CreatedAt < currentMonthStart)
            .GroupBy(x => x.oi.ProductVarientId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => (long)(x.oi.Count ?? 0)) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var variants = await context.ProductVariants
            .IgnoreQueryFilters()
            .Include(pv => pv.Product)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return variants.Select(
            pv => new ProductReportResponse
            {
                ProductName = pv.Product?.Name,
                VariantId = pv.Id,
                StockQuantity =
                    (int)((confirmedInputs.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0) -
                            (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0)),
                SoldLastMonth = (int)(soldLastMonth.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0)
            });
    }

    public async Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(
        int variantId,
        CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants
            .FirstOrDefaultAsync(pv => pv.Id == variantId, cancellationToken)
            .ConfigureAwait(false);

        if(variant is null)
        {
            return null;
        }

        var totalInput = await context.InputInfos
                .Join(context.InputReceipts, ii => ii.InputId, i => i.Id, (ii, i) => new { ii, i })
                .Where(x => x.ii.ProductId == variantId && x.i.StatusId == Domain.Constants.Input.InputStatus.Finish)
                .SumAsync(x => (long?)(x.ii.Count ?? 0), cancellationToken)
                .ConfigureAwait(false) ??
            0;

        var totalOutput = await context.OutputInfos
                .Join(context.OutputOrders, oi => oi.OutputId, o => o.Id, (oi, o) => new { oi, o })
                .Where(x => x.oi.ProductVarientId == variantId && x.o.StatusId != OrderStatus.Cancelled)
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
