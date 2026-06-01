using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants;
using Domain.Constants.Order;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Statistical
{
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
            return [.. revenueData.GroupBy(r => r.BrandName)
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

        public Task<DashboardStatsResponse?> GetDashboardStatsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

        public Task<IEnumerable<ProductReportResponse>> GetProductReportLastMonthAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductPerformanceTableResponse>> GetProductPerformanceTableAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<WarehouseTableDataResponse>> GetWarehouseTableDataAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ProductStockPriceResponse?> GetProductStockAndPriceAsync(int variantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
