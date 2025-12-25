namespace Application.ApiContracts.Statistical.Responses
{
    public class DailyRevenueResponse
    {
        public DateOnly ReportDay { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
