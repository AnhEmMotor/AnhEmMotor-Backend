namespace Application.Features.ChatTools.Queries.GetProductPriceListForChat;

public record ChatProductPriceListItemDto
{
    public string ProductName { get; init; } = string.Empty;

    public decimal SellPrice { get; init; }

    public string Currency { get; init; } = "VND";
}
