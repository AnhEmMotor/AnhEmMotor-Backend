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

public class TopProductRevenueResponse
{
    public string? ProductName { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class BrandRevenueResponse
{
    public string? BrandName { get; set; }
    public decimal Revenue { get; set; }
}

public class PaymentMethodDistributionResponse
{
    public string? MethodName { get; set; }
    public decimal Value { get; set; }
}

public class DailyRevenueTableResponse
{
    public DateOnly ReportDay { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
    public double Growth { get; set; }
    public bool HasZeroCostPrice { get; set; }
}
