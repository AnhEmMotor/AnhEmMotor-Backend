using Application.DTOs.Analytics;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDBContext _context;

        public AnalyticsRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime start, DateTime end)
        {
            var totalRevenue = await _context.OutputOrders
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));
            var totalExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
                .SumAsync(e => e.Amount);
            var cogs = totalRevenue * 0.7m;
            var grossProfit = totalRevenue - cogs;
            var netProfit = grossProfit - totalExpenses;
            var pendingAmount = await _context.OutputOrders
                .Where(o => o.StatusId == "Pending" || o.StatusId == "WaitingForPayment")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));
            var channelRaw = await _context.OutputInfos
                .IgnoreQueryFilters()
                .Join(
                    _context.OutputOrders.IgnoreQueryFilters(),
                    oi => oi.OutputId,
                    o => o.Id,
                    (oi, o) => new { oi, o })
                .Where(x => x.o.CreatedAt >= start && x.o.CreatedAt <= end && x.o.StatusId == "Completed")
                .Select(
                    x => new
                    {
                        CategoryName = x.oi.ProductVariant != null &&
                                    x.oi.ProductVariant.Product != null &&
                                    x.oi.ProductVariant.Product.ProductCategory != null
                            ? x.oi.ProductVariant.Product.ProductCategory.Name
                            : "Khác",
                        Revenue = (x.oi.Price ?? 0M) * (x.oi.Count ?? 0),
                        OrderId = x.o.Id
                    })
                .ToListAsync();
            var channelData = channelRaw
                .GroupBy(x => x.CategoryName)
                .Select(
                    g =>
                    {
                        var ordersCount = g.Select(x => x.OrderId).Distinct().Count();
                        var visits = ordersCount * 5 + 12;
                        return new ChannelDataDto
                        {
                            Name = g.Key ?? "Unknown",
                            Amount = g.Sum(x => x.Revenue),
                            Orders = ordersCount,
                            Visits = visits,
                            ConversionRate = visits > 0 ? Math.Round((decimal)ordersCount / visits * 100, 1) : 0,
                            ChangePercent = 2.5m
                        };
                    })
                .ToList();
            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalExpense = totalExpenses,
                GrossProfit = grossProfit,
                NetProfit = netProfit,
                PendingAmount = pendingAmount,
                AlertsCount = 0,
                MonthAchieved = totalRevenue,
                MonthTarget = 1000000000m,
                MonthRemaining = 1000000000m - totalRevenue,
                MonthForecast = totalRevenue * 1.2m,
                ChannelData = channelData
            };
        }

        public async Task<PnlReportDto> GetPnlReportAsync(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var revenue = await _context.OutputOrders
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));
            var expenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
                .ToListAsync();
            var totalExpenses = expenses.Sum(e => e.Amount);
            var cogs = revenue * 0.7m;
            return new PnlReportDto
            {
                Period = $"Tháng {month}/{year}",
                TotalRevenue = revenue,
                TotalCostOfGoodsSold = cogs,
                TotalOperatingExpenses = totalExpenses,
                GrossProfit = revenue - cogs,
                NetProfit = revenue - cogs - totalExpenses,
                ExpenseDetails =
                    expenses.Select(
                        e => new ExpenseDetailDto
                        {
                            Category = e.Category == ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
                            Amount = e.Amount
                        })
                        .ToList()
            };
        }

        public async Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime start, DateTime end)
        {
            var staffSales = await _context.EmployeeProfiles
                .Include(e => e.User)
                .Select(
                    e => new
                    {
                        e.Id,
                        FullName = e.User.FullName ?? e.User.UserName,
                        Role = e.JobTitle,
                        Sales = _context.OutputOrders
                            .Where(
                                o => o.FinishedBy == e.User.Id &&
                                        o.CreatedAt >= start &&
                                        o.CreatedAt <= end &&
                                        o.StatusId == "Completed")
                            .SelectMany(o => o.OutputInfos)
                            .Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                        HasSalesData = _context.OutputOrders.Any(
                            o => o.FinishedBy == e.User.Id &&
                                o.CreatedAt >= start &&
                                o.CreatedAt <= end &&
                                o.StatusId == "Completed")
                    })
                .ToListAsync();
            var employeeIds = staffSales.Select(s => s.Id).ToList();
            var kpis = await _context.KPIs
                .Where(
                    k => employeeIds.Contains(k.EmployeeProfileId) &&
                        k.PeriodStart <= end &&
                        k.PeriodEnd >= start)
                .OrderByDescending(k => k.PeriodStart)
                .ToListAsync();
            var commissions = await _context.CommissionRecords
                .Where(
                    cr => employeeIds.Contains(cr.EmployeeProfileId) &&
                        cr.DateEarned >= start &&
                        cr.DateEarned <= end)
                .ToListAsync();
            var result = new List<StaffPerformanceDto>();
            foreach (var s in staffSales)
            {
                var kpi = kpis.FirstOrDefault(k => k.EmployeeProfileId == s.Id);
                var employeeCommissions = commissions
                    .Where(cr => cr.EmployeeProfileId == s.Id)
                    .ToList();
                var commissionPaid = employeeCommissions
                    .Where(
                        cr => cr.Status is CommissionStatus.Confirmed or CommissionStatus.Paid)
                    .Sum(cr => cr.Amount);
                var totalSales = s.HasSalesData
                    ? s.Sales
                    : kpi?.ActualValue ?? 0;
                result.Add(
                    new StaffPerformanceDto
                    {
                        EmployeeName = s.FullName ?? string.Empty,
                        Role = s.Role ?? string.Empty,
                        TotalSales = totalSales,
                        TargetSales = kpi?.TargetValue ?? 0,
                        CommissionPaid = commissionPaid,
                        KpiStatus = kpi == null
                            ? "Chưa đặt KPI"
                            : GetKpiStatus(totalSales, kpi.TargetValue),
                        HasSalesData = s.HasSalesData,
                        HasKpiData = kpi != null,
                        HasCommissionData = employeeCommissions.Count > 0,
                        SalesSource = s.HasSalesData
                            ? "Đơn hàng đã hoàn tất"
                            : kpi != null
                                ? "Giá trị thực tế từ KPI"
                                : "Chưa có dữ liệu nguồn",
                        IsTopSeller = false
                    });
            }
            var maxSales = result.Count > 0 ? result.Max(r => r.TotalSales) : 0;
            if (maxSales > 0)
            {
                foreach (var r in result)
                {
                    r.IsTopSeller = r.TotalSales == maxSales;
                }
            }
            return result;
        }

        private static string GetKpiStatus(decimal totalSales, decimal targetSales)
        {
            if (targetSales <= 0)
            {
                return "Chưa đặt KPI";
            }

            if (totalSales > targetSales)
            {
                return "Vượt KPI";
            }

            return totalSales == targetSales ? "Đạt" : "Cần cải thiện";
        }

        public async Task<List<TransactionLogDto>> GetRecentTransactionsAsync(int limit = 50)
        {
            var orders = await _context.OutputOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .Select(
                    o => new TransactionLogDto
                    {
                        Timestamp = o.CreatedAt.HasValue ? o.CreatedAt.Value.DateTime : DateTime.MinValue,
                        CustomerName = o.CustomerName ?? string.Empty,
                        ProductName =
                            o.OutputInfos
                                        .Select(
                                            oi => oi.ProductVariant != null && oi.ProductVariant.Product != null
                                                            ? oi.ProductVariant.Product.Name
                                                            : "N/A")
                                        .FirstOrDefault() ??
                                    "N/A",
                        Amount = o.OutputInfos.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                        IsRevenue = true,
                        StaffName = "N/A"
                    })
                .ToListAsync();
            var expenses = await _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(limit)
                .Select(
                    e => new TransactionLogDto
                    {
                        Timestamp = e.ExpenseDate,
                        CustomerName = "Hệ thống",
                        ProductName = e.Name,
                        Amount = e.Amount,
                        IsRevenue = false,
                        StaffName = "Admin"
                    })
                .ToListAsync();
            return orders.Concat(expenses).OrderByDescending(t => t.Timestamp).ToList();
        }
    }
}
