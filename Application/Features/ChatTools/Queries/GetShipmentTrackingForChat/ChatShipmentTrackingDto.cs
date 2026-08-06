namespace Application.Features.ChatTools.Queries.GetShipmentTrackingForChat;

public record ChatShipmentTrackingDto
{
    public int OrderId { get; init; }

    public string TrackingNumber { get; init; } = string.Empty;

    public string Carrier { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? DeliveredAt { get; init; }
}
