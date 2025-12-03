namespace Application.ApiContracts.Staticals
{
    public class DailyRevenueResponse
    {
        public DateOnly ReportDay { get; set; }

        public long TotalRevenue { get; set; }
    }
}
