namespace Application.Features.ChatTools.Queries.GetLowStockProductsForChat;

public record ChatLowStockProductDto
{
    public string ProductName { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public decimal SellPrice { get; init; }

    public string Currency { get; init; } = "VND";

    public string Status { get; init; } = string.Empty;
}
