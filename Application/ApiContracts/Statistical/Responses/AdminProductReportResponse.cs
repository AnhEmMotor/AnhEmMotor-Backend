namespace Application.ApiContracts.Statistical.Responses;

public class AdminProductReportResponse
{
    public ProductReportHighlightsResponse Highlights { get; set; } = new();

    public IEnumerable<TopProductRevenueResponse> TopRevenueProducts { get; set; } = [];

    public IEnumerable<TopProductProfitResponse> TopProfitProducts { get; set; } = [];

    public IEnumerable<ProductPerformanceTableResponse> ProductPerformanceTable { get; set; } = [];
}

public class ProductReportHighlightsResponse
{
    public string? BestSellerName { get; set; }
    public int BestSellerSold { get; set; }

    public string? DeadStockName { get; set; }
    public decimal DeadStockValue { get; set; }

    public double AvgTurnover { get; set; }
    
    public int TotalSKUs { get; set; }
}

public class TopProductProfitResponse
{
    public string? ProductName { get; set; }
    public decimal Profit { get; set; }
}

public class ProductPerformanceTableResponse
{
    public string? ProductName { get; set; }
    public decimal SellPrice { get; set; }
    public int SoldCount30Days { get; set; }
    public int StockQuantity { get; set; }
    public int MaxStockQuantity { get; set; }
    public double MarginPercentage { get; set; }
    public string? Status { get; set; }
    public int[] Trend { get; set; } = [];
}
