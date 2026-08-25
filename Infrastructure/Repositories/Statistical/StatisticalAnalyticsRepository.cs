using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using Domain.Constants.Order;
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
        var endExclusive = end.TimeOfDay == TimeSpan.Zero ? end.Date.AddDays(1) : end.AddTicks(1);
        var timeSpan = endExclusive - start;
        var prevStart = start.Subtract(timeSpan);
        var prevEndExclusive = start;
        var currentTotals = await context.OutputOrders
            .Where(
                o => o.CreatedAt >= start &&
                    o.CreatedAt < endExclusive &&
                    o.StatusId != null &&
                    o.StatusId.ToLower() == OrderStatus.Completed)
            .SelectMany(o => o.OutputInfos)
            .GroupBy(_ => 1)
            .Select(
                g => new
                {
                    Revenue = g.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                    CostOfGoodsSold = g.Sum(oi => (oi.CostPrice ?? 0) * (oi.Count ?? 0))
                })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var previousTotals = await context.OutputOrders
            .Where(
                o => o.CreatedAt >= prevStart &&
                    o.CreatedAt < prevEndExclusive &&
                    o.StatusId != null &&
                    o.StatusId.ToLower() == OrderStatus.Completed)
            .SelectMany(o => o.OutputInfos)
            .GroupBy(_ => 1)
            .Select(
                g => new
                {
                    Revenue = g.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                    CostOfGoodsSold = g.Sum(oi => (oi.CostPrice ?? 0) * (oi.Count ?? 0))
                })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var totalRevenue = currentTotals?.Revenue ?? 0;
        var cogs = currentTotals?.CostOfGoodsSold ?? 0;
        var prevRevenue = previousTotals?.Revenue ?? 0;
        var prevCogs = previousTotals?.CostOfGoodsSold ?? 0;
        var totalExpenses = await context.Expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate < endExclusive)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0;
        var prevExpenses = await context.Expenses
            .Where(e => e.ExpenseDate >= prevStart && e.ExpenseDate < prevEndExclusive)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0;
        var grossProfit = totalRevenue - cogs;
        var netProfit = grossProfit - totalExpenses;
        var prevGrossProfit = prevRevenue - prevCogs;
        var prevNetProfit = prevGrossProfit - prevExpenses;
        decimal revChange = prevRevenue == 0
            ? (totalRevenue > 0 ? 100 : 0)
            : ((totalRevenue - prevRevenue) / prevRevenue * 100);
        decimal profitChange = prevNetProfit == 0
            ? (netProfit > 0 ? 100 : 0)
            : ((netProfit - prevNetProfit) / Math.Abs(prevNetProfit) * 100);
        var pendingAmount = await context.OutputOrders
            .Where(o => o.StatusId == "Pending" || o.StatusId == "WaitingForPayment")
            .SelectMany(o => o.OutputInfos)
            .SumAsync(oi => (decimal?)((oi.Price ?? 0) * (oi.Count ?? 0)), cancellationToken)
            .ConfigureAwait(false) ?? 0;
        var newComplaintsCount = await context.SupportTickets
            .CountAsync(t => t.Status == "Open" || t.Status == "New", cancellationToken)
            .ConfigureAwait(false);
        var delayedLoansCount = 0; // TODO: Implement using a properly tracked table for installments/debts
        var lowStockCount = await context.InventoryOnHands
            .CountAsync(i => i.StockQty < 10, cancellationToken)
            .ConfigureAwait(false);
        return new DashboardSummaryResponse
        {
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpenses,
            GrossProfit = grossProfit,
            NetProfit = netProfit,
            RevenueVsYesterdayPercentage = Math.Round(revChange, 1),
            DailyTarget = 100000000m,
            ProfitMargin = netProfit > 0 && totalRevenue > 0 ? Math.Round((netProfit / totalRevenue * 100), 1) : 0,
            ProfitVsYesterdayPercentage = Math.Round(profitChange, 1),
            PendingAmount = pendingAmount,
            DepositAmount = pendingAmount * 0.3m,
            LoanWaitAmount = pendingAmount * 0.7m,
            AlertsCount = newComplaintsCount + delayedLoansCount + lowStockCount,
            NewComplaintsCount = newComplaintsCount,
            DelayedLoansCount = delayedLoansCount,
            LowStockVehiclesCount = lowStockCount,
            MissedAppointmentsCount = 0,
            MonthAchieved = totalRevenue,
            MonthTarget = 1000000000m,
            MonthRemaining = Math.Max(1000000000m - totalRevenue, 0),
            MonthForecast = totalRevenue * 1.2m
        };
    }

    public async Task<PnlReportResponse> GetPnlReportAsync(int month, int year, CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        var revenue = await context.OutputOrders
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
            .SelectMany(o => o.OutputInfos)
            .SumAsync(oi => (decimal?)((oi.Price ?? 0) * (oi.Count ?? 0)), cancellationToken)
            .ConfigureAwait(false) ?? 0;
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
        var endOfDay = end.Date.AddDays(1).AddTicks(-1);
        var staffSales = await context.EmployeeProfiles
            .Include(e => e.User)
            .Select(
                e => new
                {
                    FullName = e.User != null ? e.User.FullName : "N/A",
                    Role = e.JobTitle,
                    Sales = context.OutputOrders
                        .Where(
                            o => e.User != null &&
                                    o.CreatedBy == e.User.Id &&
                                    o.CreatedAt >= start &&
                                    o.CreatedAt <= endOfDay &&
                                    o.StatusId != null &&
                                    o.StatusId.ToLower() == OrderStatus.Completed)
                        .SelectMany(o => o.OutputInfos)
                        .Sum(oi => (decimal?)((oi.Price ?? 0) * (oi.Count ?? 0))) ?? 0,
                    TargetSales = context.KPIs
                        .Where(
                            k => k.EmployeeProfileId == e.Id &&
                                    k.PeriodStart <= endOfDay &&
                                    k.PeriodEnd >= start)
                        .Sum(k => (decimal?)k.TargetValue) ?? 0,
                    CommissionPaid = context.CommissionRecords
                        .Where(
                            c => c.EmployeeProfileId == e.Id &&
                                    c.DateEarned >= start &&
                                    c.DateEarned <= endOfDay &&
                                    (c.Status == Domain.Entities.CommissionStatus.Confirmed || c.Status == Domain.Entities.CommissionStatus.Paid))
                        .Sum(c => (decimal?)c.Amount) ?? 0
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var sortedStaffSales = staffSales.OrderByDescending(s => s.Sales).ToList();

        return [.. sortedStaffSales.Select(
            (s, index) => new StaffPerformanceResponse
            {
                EmployeeName = s.FullName ?? string.Empty,
                Role = s.Role ?? string.Empty,
                TotalSales = s.Sales,
                TargetSales = s.TargetSales,
                CommissionPaid = s.CommissionPaid,
                KpiStatus = s.TargetSales == 0 ? "Chưa đặt KPI" :
                            (s.Sales >= s.TargetSales * 1.1m ? "Vượt KPI" :
                            (s.Sales >= s.TargetSales ? "Đạt" : "Cần cải thiện")),
                IsTopSeller = index == 0 && s.Sales > 0
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
                    Status = o.StatusId != null && o.StatusId.ToLower() == OrderStatus.Completed
                        ? "Completed"
                        : "Pending",
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
