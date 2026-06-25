using Application.DTOs.Analytics;
using System;

namespace Infrastructure.Repositories
{
    public interface IAnalyticsRepository
    {
        public Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime start, DateTime end);

        public Task<PnlReportDto> GetPnlReportAsync(int month, int year);

        public Task<List<StaffPerformanceDto>> GetStaffPerformanceAsync(DateTime start, DateTime end);

        public Task<List<TransactionLogDto>> GetRecentTransactionsAsync(int limit = 50);
    }

    public class StaffPerformanceDto
    {
        public string EmployeeName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public decimal TotalSales { get; set; }

        public decimal CommissionPaid { get; set; }

        public string KpiStatus { get; set; } = string.Empty;
    }

    public class TransactionLogDto
    {
        public DateTime Timestamp { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsRevenue { get; set; }

        public string StaffName { get; set; } = string.Empty;
    }
}
