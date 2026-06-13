using System;
using System.Collections.Generic;
using System.Text;

namespace Application.ApiContracts.Statistical.Responses
{
    public class PnlReportResponse
    {
        public string Period { get; set; } = string.Empty; // Ví dụ: "Tháng 05/2026"
        public decimal TotalRevenue { get; set; }          // Tổng thu
        public decimal TotalCostOfGoodsSold { get; set; }  // Giá vốn hàng bán
        public decimal TotalOperatingExpenses { get; set; } // Tổng chi phí vận hành
        public decimal GrossProfit { get; set; }           // Lợi nhuận gộp
        public decimal NetProfit { get; set; }             // Lợi nhuận ròng

        public List<ExpenseDetailResponse> ExpenseDetails { get; set; } = [];
    }
}
