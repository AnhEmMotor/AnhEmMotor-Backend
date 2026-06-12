using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DBContexts;
using AnhEmMotor.Application.DTOs.Analytics;
using Domain.Entities;
using AnhEmMotor.Domain.Enums;

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
            // 1. Doanh thu thực tế (Completed)
            var totalRevenue = await _context.OutputOrders
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));

            // 2. Chi phí vận hành trong kỳ
            var totalExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end)
                .SumAsync(e => e.Amount);

            // 3. Giá vốn hàng bán (COGS) - Giả định lấy từ giá nhập của sản phẩm trong đơn hàng
            // Ở đây tôi tạm tính COGS = 70% Doanh thu nếu chưa có bảng giá vốn chi tiết
            var cogs = totalRevenue * 0.7m;

            // 4. Tiền đang treo (Pending)
            var pendingAmount = await _context.OutputOrders
                .Where(o => o.StatusId == "Pending" || o.StatusId == "WaitingForPayment")
                .SelectMany(o => o.OutputInfos)
                .SumAsync(oi => (oi.Price ?? 0) * (oi.Count ?? 0));

            return new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                RevenueVsYesterdayPercentage = 12.5m, // Mock
                DailyTarget = 100000000m, // 100tr

                NetProfit = totalRevenue - cogs - totalExpenses,
                ProfitMargin = 21.0m, // Mock
                ProfitVsYesterdayPercentage = -3.0m, // Mock

                PendingAmount = pendingAmount,
                DepositAmount = pendingAmount * 0.3m, // Mock 30%
                LoanWaitAmount = pendingAmount * 0.7m, // Mock 70%

                AlertsCount = 6, // Mock sum
                NewComplaintsCount = 2,
                DelayedLoansCount = 1,
                LowStockVehiclesCount = 3,
                MissedAppointmentsCount = 0,

                MonthAchieved = totalRevenue,
                MonthTarget = 1000000000m, // Ví dụ 1 tỷ
                MonthRemaining = 1000000000m - totalRevenue,
                MonthForecast = totalRevenue * 1.2m // Giả định dự báo
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
                ExpenseDetails = expenses.Select(e => new ExpenseDetailDto
                {
                    Category = e.Category == ExpenseCategory.Fixed ? "Cố định" : "Biến đổi",
                    Amount = e.Amount
                }).ToList()
            };
        }

        public async Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime start, DateTime end)
        {
            var staffSales = await _context.EmployeeProfiles
                .Include(e => e.User)
                .Select(e => new
                {
                    FullName = e.User.UserName, // Giả định dùng UserName làm FullName nếu không có field riêng
                    Role = e.JobTitle,
                    Sales = _context.OutputOrders
                        .Where(o => o.FinishedBy == e.User.Id && o.CreatedAt >= start && o.CreatedAt <= end && o.StatusId == "Completed")
                        .SelectMany(o => o.OutputInfos)
                        .Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0))
                })
                .ToListAsync();

            return staffSales.Select((s, index) => new StaffPerformanceDto
            {
                EmployeeName = s.FullName ?? string.Empty,
                Role = s.Role ?? string.Empty,
                TotalSales = s.Sales,
                TargetSales = 120000000m, // Mock 120tr
                CommissionPaid = s.Sales * 0.02m, // Giả định 2% hoa hồng
                KpiStatus = s.Sales > 100000000 ? "Vượt KPI" : (s.Sales > 50000000 ? "Đạt" : "Cần cải thiện"),
                IsTopSeller = index == 0 // Mock top 1
            }).ToList();
        }

        public async Task<List<TransactionLogDto>> GetRecentTransactionsAsync(int limit = 50)
        {
            // Gộp đơn hàng và chi phí vào một luồng timeline
            var orders = await _context.OutputOrders
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit)
                .Select(o => new TransactionLogDto
                {
                    Timestamp = o.CreatedAt.HasValue ? o.CreatedAt.Value.DateTime : DateTime.MinValue,
                    CustomerName = o.CustomerName ?? string.Empty,
                    ProductName = o.OutputInfos.Select(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null ? oi.ProductVariant.Product.Name : "N/A").FirstOrDefault() ?? "N/A",
                    Amount = o.OutputInfos.Sum(oi => (oi.Price ?? 0) * (oi.Count ?? 0)),
                    IsRevenue = true,
                    Status = o.StatusId == "Completed" ? "Completed" : "Pending", // Tạm dùng StatusId
                    StaffName = "N/A"
                }).ToListAsync();

            var expenses = await _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Take(limit)
                .Select(e => new TransactionLogDto
                {
                    Timestamp = e.ExpenseDate,
                    CustomerName = "Hệ thống",
                    ProductName = e.Name,
                    Amount = e.Amount,
                    IsRevenue = false,
                    Status = "Refund", // Tạm gọi khoản chi là Refund cho demo UI
                    StaffName = "Admin"
                }).ToListAsync();

            return orders.Concat(expenses).OrderByDescending(t => t.Timestamp).ToList();
        }
    }
}
