namespace Application.ApiContracts.Statistical.Responses;

public class AdminProductReportResponse
{
    public ProductReportHighlightsResponse Highlights { get; set; } = new();

    public IEnumerable<TopProductRevenueResponse> TopRevenueProducts { get; set; } = [];

    public IEnumerable<TopProductProfitResponse> TopProfitProducts { get; set; } = [];

    public IEnumerable<ProductPerformanceTableResponse> ProductPerformanceTable { get; set; } = [];
}


