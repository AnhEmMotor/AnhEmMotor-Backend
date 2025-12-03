namespace Application.ApiContracts.Staticals
{
    public class DashboardStatsResponse
    {
        public long LastMonthRevenue { get; set; }

        public long LastMonthProfit { get; set; }

        public long PendingOrdersCount { get; set; }

        public long NewCustomersCount { get; set; }
    }
}
