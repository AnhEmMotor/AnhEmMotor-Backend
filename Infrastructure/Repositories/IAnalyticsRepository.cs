using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnhEmMotor.Application.DTOs.Analytics;

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
        public decimal TargetSales { get; set; }
        public decimal CommissionPaid { get; set; }
        public string KpiStatus { get; set; } = string.Empty; // Vượt KPI, Đạt, Cần cải thiện
        public bool IsTopSeller { get; set; }
    }

    public class TransactionLogDto
    {
        public DateTime Timestamp { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsRevenue { get; set; } // True: Thu, False: Chi
        public string Status { get; set; } = string.Empty; // Completed, Pending, Refund
        public string StaffName { get; set; } = string.Empty;
    }
}
