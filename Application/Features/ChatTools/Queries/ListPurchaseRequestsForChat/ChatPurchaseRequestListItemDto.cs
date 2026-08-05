namespace Application.Features.ChatTools.Queries.ListPurchaseRequestsForChat;

public record ChatPurchaseRequestListItemDto
{
    public int PurchaseRequestId { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? Note { get; init; }

    public string? CreatedByName { get; init; }

    public int TotalItems { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}
