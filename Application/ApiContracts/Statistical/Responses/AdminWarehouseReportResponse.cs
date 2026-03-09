namespace Application.ApiContracts.Statistical.Responses;

public class AdminWarehouseReportResponse
{
    public WarehouseSummaryResponse Summary { get; set; } = new();

    public IEnumerable<BrandStockResponse> StockByBrand { get; set; } = [];

    public IEnumerable<StockStatusRatioResponse> StockStatusRatio { get; set; } = [];

    public IEnumerable<WarehouseTableDataResponse> WarehouseTableData { get; set; } = [];
}

public class WarehouseSummaryResponse
{
    public int TotalStock { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class BrandStockResponse
{
    public string? BrandName { get; set; }
    public int InStock { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
}

public class StockStatusRatioResponse
{
    public string? StatusLabel { get; set; }
    public int Count { get; set; }
}

public class WarehouseTableDataResponse
{
    public string? BrandName { get; set; }
    public int TotalStock { get; set; }
    public int Capacity { get; set; }
    public int LowStock { get; set; }
    public int OutOfStock { get; set; }
    public string? Status { get; set; }
    public decimal Value { get; set; }
}
