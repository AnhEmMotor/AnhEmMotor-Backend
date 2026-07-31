namespace Application.Features.ChatTools.Queries.GetPurchaseRequestDetailForChat;

public record ChatPurchaseRequestDetailDto
{
    public int PurchaseRequestId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? Note { get; init; }

    public string? CreatedByName { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public IReadOnlyList<ChatPurchaseRequestDetailItemDto> Items { get; init; } = [];
}

public record ChatPurchaseRequestDetailItemDto
{
    public string? ProductName { get; init; }

    public int Quantity { get; init; }

    public string? SupplierName { get; init; }

    public decimal? UnitPrice { get; init; }
}
