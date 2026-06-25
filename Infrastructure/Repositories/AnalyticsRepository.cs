using Application.DTOs.Analytics;
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
            var pendingAmount = await _context.OutputOrders
                .Where(o => o.StatusId == "Pending" || o.StatusId == "WaitingForPayment")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));
            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                NetProfit = totalRevenue - cogs - totalExpenses,
                PendingAmount = pendingAmount,
                AlertsCount = 0,
                MonthAchieved = totalRevenue,
                MonthTarget = 1000000000m,
                MonthRemaining = 1000000000m - totalRevenue,
                MonthForecast = totalRevenue * 1.2m
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
                        FullName = e.User.FullName ?? e.User.UserName,
                        Role = e.JobTitle,
                        Sales = _context.OutputOrders
                            .Where(
                                o => o.FinishedBy == e.User.Id &&
                                        o.CreatedAt >= start &&
                                        o.CreatedAt <= end &&
                                        o.StatusId == "Completed")
                            .SelectMany(o => o.OutputInfos)
                            .Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0))
                    })
                .ToListAsync();
            return staffSales.Select(
                s => new StaffPerformanceDto
                {
                    EmployeeName = s.FullName ?? string.Empty,
                    Role = s.Role ?? string.Empty,
                    TotalSales = s.Sales,
                    CommissionPaid = s.Sales * 0.02m,
                    KpiStatus = s.Sales > 100000000 ? "Vượt KPI" : (s.Sales > 50000000 ? "Đạt" : "Cần cải thiện")
                })
                .ToList();
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
