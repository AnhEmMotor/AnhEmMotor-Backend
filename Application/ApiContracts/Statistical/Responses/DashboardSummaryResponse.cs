using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class DashboardSummaryResponse
    {
        public decimal TotalRevenue { get; set; }

        public decimal TotalExpense { get; set; } // Tổng chi phí

        public decimal GrossProfit { get; set; } // Lợi nhuận gộp (Revenue - COGS)

        public decimal NetProfit { get; set; } // Lợi nhuận ròng

        public decimal RevenueVsYesterdayPercentage { get; set; }

        public decimal DailyTarget { get; set; }

        public decimal ProfitMargin { get; set; }

        public decimal ProfitVsYesterdayPercentage { get; set; }

        public decimal PendingAmount { get; set; }

        public decimal DepositAmount { get; set; }

        public decimal LoanWaitAmount { get; set; }

        public int AlertsCount { get; set; }

        public int NewComplaintsCount { get; set; }

        public int DelayedLoansCount { get; set; }

        public int LowStockVehiclesCount { get; set; }

        public int MissedAppointmentsCount { get; set; }

        public decimal MonthTarget { get; set; }

        public decimal MonthAchieved { get; set; }

        public decimal MonthRemaining { get; set; }

        public decimal MonthForecast { get; set; }

        public bool IsRevenueAlert { get; set; }

        public bool IsPendingAlert { get; set; }

        public bool IsStockAlert { get; set; }
    }
}
