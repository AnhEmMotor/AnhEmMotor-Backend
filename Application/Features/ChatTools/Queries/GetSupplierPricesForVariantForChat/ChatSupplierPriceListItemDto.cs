namespace Application.Features.ChatTools.Queries.GetSupplierPricesForVariantForChat;

public record ChatSupplierPriceListItemDto
{
    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public int VariantId { get; init; }

    public int? ColorId { get; init; }

    public int QuotePrice { get; init; }

    public string Currency { get; init; } = "VND";

    public string? Note { get; init; }
}
