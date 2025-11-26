using Application.Interfaces.Repositories.Statistical;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalReadRepository(ApplicationDBContext context) : IStatisticalReadRepository
{
    public async Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(
        int days,
        CancellationToken cancellationToken)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(days - 1)));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var dateSeries = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i))
            .ToList();

        var revenueData = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled && x.o.CreatedAt != null)
            .GroupBy(x => DateOnly.FromDateTime(x.o.CreatedAt!.Value.UtcDateTime))
            .Select(g => new
            {
                Day = g.Key,
                Revenue = g.Sum(x => (long)(x.oi.Price ?? 0) * (x.oi.Count ?? 0))
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return dateSeries.Select(day => new DailyRevenueDto
        {
            ReportDay = day,
            TotalRevenue = revenueData.FirstOrDefault(r => r.Day == day)?.Revenue ?? 0
        });
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync(
        CancellationToken cancellationToken)
    {
        var lastMonthStart = new DateTimeOffset(DateTime.UtcNow.AddMonths(-1).Year, DateTime.UtcNow.AddMonths(-1).Month, 1, 0, 0, 0, TimeSpan.Zero);
        var currentMonthStart = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var lastMonthRevenue = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.CreatedAt >= lastMonthStart
                && x.o.CreatedAt < currentMonthStart
                && x.o.StatusId != OrderStatus.Cancelled)
            .SumAsync(x => (long?)(x.oi.Price ?? 0) * (x.oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false) ?? 0;

        var lastMonthProfit = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.CreatedAt >= lastMonthStart
                && x.o.CreatedAt < currentMonthStart
                && x.o.StatusId != OrderStatus.Cancelled)
            .SumAsync(x => (long?)((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false) ?? 0;

        var pendingOrdersCount = await context.OutputOrders
            .Where(o => o.StatusId == OrderStatus.Pending)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new DashboardStatsDto
        {
            LastMonthRevenue = lastMonthRevenue,
            LastMonthProfit = lastMonthProfit,
            PendingOrdersCount = pendingOrdersCount,
            NewCustomersCount = 0
        };
    }

    public async Task<IEnumerable<MonthlyRevenueProfitDto>> GetMonthlyRevenueProfitAsync(
        int months,
        CancellationToken cancellationToken)
    {
        var currentMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var startMonth = currentMonth.AddMonths(-(months - 1));

        var monthSeries = Enumerable.Range(0, months)
            .Select(i => startMonth.AddMonths(i))
            .ToList();

        var revenueData = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled && x.o.CreatedAt != null)
            .GroupBy(x => new DateOnly(x.o.CreatedAt!.Value.Year, x.o.CreatedAt.Value.Month, 1))
            .Select(g => new
            {
                Month = g.Key,
                Revenue = g.Sum(x => (long)(x.oi.Price ?? 0) * (x.oi.Count ?? 0)),
                Profit = g.Sum(x => (long)((x.oi.Price ?? 0) - (x.oi.CostPrice ?? 0)) * (x.oi.Count ?? 0))
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return monthSeries.Select(month => new MonthlyRevenueProfitDto
        {
            ReportMonth = month,
            TotalRevenue = revenueData.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0,
            TotalProfit = revenueData.FirstOrDefault(r => r.Month == month)?.Profit ?? 0
        });
    }

    public async Task<IEnumerable<OrderStatusCountDto>> GetOrderStatusCountsAsync(
        CancellationToken cancellationToken)
    {
        return await context.OutputStatuses
            .GroupJoin(
                context.OutputOrders,
                os => os.Key,
                o => o.StatusId,
                (os, orders) => new OrderStatusCountDto
                {
                    StatusName = os.Key,
                    OrderCount = orders.Count()
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ProductReportDto>> GetProductReportLastMonthAsync(
        CancellationToken cancellationToken)
    {
        var lastMonthStart = new DateTimeOffset(DateTime.UtcNow.AddMonths(-1).Year, DateTime.UtcNow.AddMonths(-1).Month, 1, 0, 0, 0, TimeSpan.Zero);
        var currentMonthStart = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var confirmedInputs = await context.InputInfos
            .Join(
                context.InputReceipts,
                ii => ii.InputId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(x => x.i.StatusId == InputStatus.Finish)
            .GroupBy(x => x.ii.ProductId)
            .Select(g => new
            {
                VariantId = g.Key,
                TotalIn = g.Sum(x => (long)(x.ii.Count ?? 0))
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soldOutputsAll = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled)
            .GroupBy(x => x.oi.ProductId)
            .Select(g => new
            {
                VariantId = g.Key,
                TotalOut = g.Sum(x => (long)(x.oi.Count ?? 0))
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var soldLastMonth = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.o.StatusId != OrderStatus.Cancelled
                && x.o.CreatedAt >= lastMonthStart
                && x.o.CreatedAt < currentMonthStart)
            .GroupBy(x => x.oi.ProductId)
            .Select(g => new
            {
                VariantId = g.Key,
                TotalSold = g.Sum(x => (long)(x.oi.Count ?? 0))
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var variants = await context.ProductVariants
            .Include(pv => pv.Product)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return variants.Select(pv => new ProductReportDto
        {
            ProductName = pv.Product?.Name,
            VariantId = pv.Id,
            StockQuantity = (confirmedInputs.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalIn ?? 0)
                - (soldOutputsAll.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalOut ?? 0),
            SoldLastMonth = soldLastMonth.FirstOrDefault(x => x.VariantId == pv.Id)?.TotalSold ?? 0
        });
    }

    public async Task<ProductStockPriceDto?> GetProductStockAndPriceAsync(
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
            .Join(
                context.InputReceipts,
                ii => ii.InputId,
                i => i.Id,
                (ii, i) => new { ii, i })
            .Where(x => x.ii.ProductId == variantId && x.i.StatusId == InputStatus.Finish)
            .SumAsync(x => (long?)(x.ii.Count ?? 0), cancellationToken)
            .ConfigureAwait(false) ?? 0;

        var totalOutput = await context.OutputInfos
            .Join(
                context.OutputOrders,
                oi => oi.OutputId,
                o => o.Id,
                (oi, o) => new { oi, o })
            .Where(x => x.oi.ProductId == variantId && x.o.StatusId != OrderStatus.Cancelled)
            .SumAsync(x => (long?)(x.oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false) ?? 0;

        return new ProductStockPriceDto
        {
            UnitPrice = variant.Price ?? 0,
            StockQuantity = totalInput - totalOutput
        };
    }
}
