
namespace Application.ApiContracts.Statistical.Responses;

public class AdminRevenueAnalysisResponse
{
    public DashboardStatsResponse Summary { get; set; } = new();

    public IEnumerable<DailyRevenueResponse> RevenueTrend { get; set; } = [];

    public IEnumerable<TopProductRevenueResponse> TopProductsByRevenue { get; set; } = [];

    public IEnumerable<BrandRevenueResponse> BrandRevenueDistribution { get; set; } = [];

    public IEnumerable<PaymentMethodDistributionResponse> PaymentMethodDistribution { get; set; } = [];

    public IEnumerable<DailyRevenueTableResponse> DailyTableData { get; set; } = [];
}

