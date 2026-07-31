namespace Application.Features.ChatTools.Queries.GetActiveShipmentsForChat;

public record ChatActiveShipmentListItemDto
{
    public int Id { get; init; }

    public string TrackingNumber { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public string Carrier { get; init; } = string.Empty;

    public decimal CodAmount { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public int DaysInTransit { get; init; }
}
