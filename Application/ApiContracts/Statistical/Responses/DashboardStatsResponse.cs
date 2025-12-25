namespace Application.ApiContracts.Statistical.Responses
{
    public class DashboardStatsResponse
    {
        public decimal LastMonthRevenue { get; set; }

        public decimal LastMonthProfit { get; set; }

        public int PendingOrdersCount { get; set; }

        public int NewCustomersCount { get; set; }
    }
}
