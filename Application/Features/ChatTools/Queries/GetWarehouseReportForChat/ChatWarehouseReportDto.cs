namespace Application.Features.ChatTools.Queries.GetWarehouseReportForChat;

public record ChatWarehouseReportDto
{
    public int TotalStock { get; init; }

    public decimal TotalValue { get; init; }

    public int LowStockCount { get; init; }

    public int OutOfStockCount { get; init; }

    public IReadOnlyList<ChatWarehouseBrandStockDto> StockByBrand { get; init; } = [];

    public string Currency { get; init; } = "VND";
}

public record ChatWarehouseBrandStockDto
{
    public string? BrandName { get; init; }

    public int StockCount { get; init; }

    public int InStock { get; init; }

    public int LowStock { get; init; }

    public int OutOfStock { get; init; }
}
