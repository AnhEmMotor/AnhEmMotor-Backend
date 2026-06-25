namespace Application.DTOs.Analytics
{
    public class PnlReportDto
    {
        public string Period { get; set; } = string.Empty;

        public decimal TotalRevenue { get; set; }

        public decimal TotalCostOfGoodsSold { get; set; }

        public decimal TotalOperatingExpenses { get; set; }

        public decimal GrossProfit { get; set; }

        public decimal NetProfit { get; set; }

        public List<ExpenseDetailDto> ExpenseDetails { get; set; } = new();
    }

    public class ExpenseDetailDto
    {
        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
