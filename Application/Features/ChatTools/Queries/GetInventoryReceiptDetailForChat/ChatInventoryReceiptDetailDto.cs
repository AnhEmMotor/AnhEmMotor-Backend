namespace Application.Features.ChatTools.Queries.GetInventoryReceiptDetailForChat;

public record ChatInventoryReceiptDetailDto
{
    public int? Id { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public string? StatusId { get; init; }
    public string? SupplierName { get; init; }
    public string? CreatedByName { get; init; }
    public string? Notes { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "VND";
    public List<ChatInventoryReceiptDetailItemDto> Items { get; init; } = [];
}

public record ChatInventoryReceiptDetailItemDto
{
    public string? Name { get; init; }
    public string? ColorName { get; init; }
    public int? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
}
