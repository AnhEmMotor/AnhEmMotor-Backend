using System;
using System.Collections.Generic;

namespace Application.DTOs.Analytics
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }        // Doanh thu thực tế (Completed)
        public decimal NetProfit { get; set; }           // Lợi nhuận ròng
        public decimal PendingAmount { get; set; }       // Tiền đang treo
        public int AlertsCount { get; set; }             // Số lượng cảnh báo

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
