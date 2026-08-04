namespace Application.Features.ChatTools.Queries.ListInventoryReceiptsForChat;

public record ChatInventoryReceiptListItemDto
{
    public int? Id { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string? StatusId { get; init; }

    public string? SupplierName { get; init; }

    public string? CreatedByName { get; init; }

    public long? TotalPayable { get; init; }

    public string Currency { get; init; } = "VND";
}
