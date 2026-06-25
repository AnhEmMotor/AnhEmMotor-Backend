using System;

namespace Application.ApiContracts.Statistical.Responses
{
    public class PnlReportResponse
    {
        public string Period { get; set; } = string.Empty;

        public decimal TotalRevenue { get; set; }

        public decimal TotalCostOfGoodsSold { get; set; }

        public decimal TotalOperatingExpenses { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal NetProfit { get; set; }

        public List<ExpenseDetailResponse> ExpenseDetails { get; set; } = [];
    }
}
