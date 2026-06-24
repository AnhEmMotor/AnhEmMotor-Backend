using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Statistical;

public class StatisticalAnalyticsRepository(ApplicationDBContext context) : IStatisticalAnalyticsRepository
{
    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken)
    {
        var totalRevenue = await context.OutputOrders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
            .SelectMany(o => o.OutputInfos)
            .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false);
        var totalExpenses = await context.Expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
            .SumAsync(e => e.Amount, cancellationToken)
            .ConfigureAwait(false);
        var cogs = totalRevenue * 0.7m;
        var pendingAmount = await context.OutputOrders
            .Where(o => o.StatusId == "Pending" || o.StatusId == "WaitingForPayment")
            .SelectMany(o => o.OutputInfos)
            .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false);
        return new DashboardSummaryResponse
        {
            TotalRevenue = totalRevenue,
            RevenueVsYesterdayPercentage = 12.5m,
            DailyTarget = 100000000m,
            NetProfit = totalRevenue - cogs - totalExpenses,
            ProfitMargin = 21.0m,
            ProfitVsYesterdayPercentage = -3.0m,
            PendingAmount = pendingAmount,
            DepositAmount = pendingAmount * 0.3m,
            LoanWaitAmount = pendingAmount * 0.7m,
            AlertsCount = 6,
            NewComplaintsCount = 2,
            DelayedLoansCount = 1,
            LowStockVehiclesCount = 3,
            MissedAppointmentsCount = 0,
            MonthAchieved = totalRevenue,
            MonthTarget = 1000000000m,
            MonthRemaining = 1000000000m - totalRevenue,
            MonthForecast = totalRevenue * 1.2m
        };
    }

    public async Task<PnlReportResponse> GetPnlReportAsync(int month, int year, CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var revenue = await context.OutputOrders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
            .SelectMany(o => o.OutputInfos)
            .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0), cancellationToken)
            .ConfigureAwait(false);
        var expenses = await context.Expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var cogs = revenue * 0.7m;
        return new PnlReportResponse
        {
            Period = $"Tháng {month}/{year}",
            TotalRevenue = revenue,
            TotalCostOfGoodsSold = cogs,
            TotalOperatingExpenses = totalExpenses,
            GrossProfit = revenue - cogs,
            NetProfit = revenue - cogs - totalExpenses,
            ExpenseDetails =
                [.. expenses.Select(
                    e => new ExpenseDetailResponse
                    {
                        Category = e.Category == ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
                        Amount = e.Amount
                    })]
        };
    }

    public async Task<List<StaffPerformanceResponse>> GetStaffPerformanceAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken)
    {
        var staffSales = await context.EmployeeProfiles
            .Include(e => e.User)
            .Select(
                e => new
                {
                    FullName = e.User.UserName,
                    Role = e.JobTitle,
                    Sales = context.OutputOrders
                        .Where(
                            o => o.FinishedBy == e.User.Id &&
                                    o.CreatedAt >= start &&
                                    o.CreatedAt <= end &&
                                    o.StatusId == "Completed")
                        .SelectMany(o => o.OutputInfos)
                        .Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0))
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return[.. staffSales.Select(
            (s, index) => new StaffPerformanceResponse
            {
                EmployeeName = s.FullName ?? string.Empty,
                Role = s.Role ?? string.Empty,
                TotalSales = s.Sales,
                TargetSales = 120000000m,
                CommissionPaid = s.Sales * 0.02m,
                KpiStatus = s.Sales > 100000000 ? "Vượt KPI" : (s.Sales > 50000000 ? "Đạt" : "Cần cải thiện"),
                IsTopSeller = index == 0
            })];
    }

    public async Task<List<TransactionLogResponse>> GetRecentTransactionsAsync(
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var orders = await context.OutputOrders
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .Select(
                o => new TransactionLogResponse
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
                    Status = o.StatusId == "Completed" ? "Completed" : "Pending",
                    StaffName = "N/A"
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var expenses = await context.Expenses
            .OrderByDescending(e => e.ExpenseDate)
            .Take(limit)
            .Select(
                e => new TransactionLogResponse
                {
                    Timestamp = e.ExpenseDate,
                    CustomerName = "Hệ thống",
                    ProductName = e.Name,
                    Amount = e.Amount,
                    IsRevenue = false,
                    Status = "Refund",
                    StaffName = "Admin"
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return[.. orders.Concat(expenses).OrderByDescending(t => t.Timestamp)];
    }
}
