namespace Application.ApiContracts.Statistical.Responses;

public class AnalyticsResponse
{
    public RevenueComparison RevenueComparison { get; set; } = new();

    public List<RevenueSourceResponse> RevenueSources { get; set; } = [];
}
