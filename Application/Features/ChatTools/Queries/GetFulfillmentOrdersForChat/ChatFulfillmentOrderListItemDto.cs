namespace Application.Features.ChatTools.Queries.GetFulfillmentOrdersForChat;

public record ChatFulfillmentOrderListItemDto
{
    public int Id { get; init; }

    public string TrackingNumber { get; init; } = string.Empty;

    public string OriginalOrderCode { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public string Carrier { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public decimal CodAmount { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? DeliveredAt { get; init; }
}
