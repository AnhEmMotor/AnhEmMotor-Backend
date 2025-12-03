namespace Application.ApiContracts.Staticals
{
    public class MonthlyRevenueProfitResponse
    {
        public DateOnly ReportMonth { get; set; }

        public long TotalRevenue { get; set; }

        public long TotalProfit { get; set; }
    }
}
