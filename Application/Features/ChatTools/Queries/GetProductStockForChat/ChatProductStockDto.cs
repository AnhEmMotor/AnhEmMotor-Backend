namespace Application.Features.ChatTools.Queries.GetProductStockForChat;

public record ChatProductStockDto
{
    public int VariantId { get; init; }

    public string? VariantName { get; init; }

    public decimal UnitPrice { get; init; }

    public int StockQuantity { get; init; }

    public string Currency { get; init; } = "VND";
}
