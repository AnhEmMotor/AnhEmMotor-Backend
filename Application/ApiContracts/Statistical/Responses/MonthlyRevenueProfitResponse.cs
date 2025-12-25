namespace Application.ApiContracts.Statistical.Responses
{
    public class MonthlyRevenueProfitResponse
    {
        public DateOnly ReportMonth { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalProfit { get; set; }
    }
}
