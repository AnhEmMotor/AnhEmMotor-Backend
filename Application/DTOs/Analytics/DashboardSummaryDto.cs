using System;
using System.Collections.Generic;

namespace AnhEmMotor.Application.DTOs.Analytics
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }        // Doanh thu thực tế (Completed)
        public decimal RevenueVsYesterdayPercentage { get; set; } // +12% so với hôm qua
        public decimal DailyTarget { get; set; }         // Mục tiêu ngày: 100tr

        public decimal NetProfit { get; set; }           // Lợi nhuận ròng
        public decimal ProfitMargin { get; set; }        // Biên lợi nhuận: 21%
        public decimal ProfitVsYesterdayPercentage { get; set; } // -3% so với hôm qua

        public decimal PendingAmount { get; set; }       // Tiền đang treo (Tổng)
        public decimal DepositAmount { get; set; }       // Cọc giữ xe
        public decimal LoanWaitAmount { get; set; }      // Chờ NH giải ngân trả góp

        public int AlertsCount { get; set; }             // Số lượng cảnh báo (Tổng)
        public int NewComplaintsCount { get; set; }      // Khiếu nại mới
        public int DelayedLoansCount { get; set; }       // NH chậm giải ngân
        public int LowStockVehiclesCount { get; set; }   // Xe sắp hết hàng
        public int MissedAppointmentsCount { get; set; } // Lịch hẹn bị bỏ lỡ

        // Tóm tắt tháng
        public decimal MonthTarget { get; set; }         // Mục tiêu tháng
        public decimal MonthAchieved { get; set; }       // Đã đạt
        public decimal MonthRemaining { get; set; }      // Cần thêm
        public decimal MonthForecast { get; set; }       // Dự báo cuối tháng

        // Trạng thái cảnh báo (cho UI đổi màu)
        public bool IsRevenueAlert { get; set; }         // < 50% lúc 15h
        public bool IsPendingAlert { get; set; }         // > 48h chưa giải ngân
        public bool IsStockAlert { get; set; }           // Tồn kho < 2
    }
}
