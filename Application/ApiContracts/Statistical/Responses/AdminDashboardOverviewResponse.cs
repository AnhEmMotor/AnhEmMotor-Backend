
namespace Application.ApiContracts.Statistical.Responses;

public class AdminDashboardOverviewResponse
{
    public DashboardStatsResponse Summary { get; set; } = new();

    public IEnumerable<OrderStatusCountResponse> OrderStatusDistribution { get; set; } = [];

    public IEnumerable<DailyRevenueResponse> DailyRevenue { get; set; } = [];

    public IEnumerable<MonthlyRevenueProfitResponse> MonthlyComparison { get; set; } = [];

    public IEnumerable<RecentOrderResponse> RecentOrders { get; set; } = [];
}
