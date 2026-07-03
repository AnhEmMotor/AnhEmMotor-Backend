using System;

namespace Application.DTOs.Analytics
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }

        public decimal TotalExpense { get; set; } // Tổng chi phí

        public decimal GrossProfit { get; set; } // Lợi nhuận gộp (Revenue - COGS)

        public decimal NetProfit { get; set; } // Lợi nhuận ròng

        public decimal PendingAmount { get; set; }

        public int AlertsCount { get; set; }

        public decimal MonthTarget { get; set; }

        public decimal MonthAchieved { get; set; }

        public decimal MonthRemaining { get; set; }

        public decimal MonthForecast { get; set; }

        public bool IsRevenueAlert { get; set; }

        public bool IsPendingAlert { get; set; }

        public bool IsStockAlert { get; set; }
    }
}
