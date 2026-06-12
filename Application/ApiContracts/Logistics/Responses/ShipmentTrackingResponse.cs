namespace Application.ApiContracts.Logistics.Responses;

public class ShipmentTrackingResponse
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string OriginalOrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal CodAmount { get; set; }
    public bool IsCodPaid { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpectedDelivery { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public List<TrackingMilestoneResponse> Milestones { get; set; } = new();
    public List<TrackingProductResponse> Products { get; set; } = new();
    public double? OriginLat { get; set; }
    public double? OriginLng { get; set; }
    public double? DestLat { get; set; }
    public double? DestLng { get; set; }
}

public class TrackingProductResponse
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ThumbnailUrl { get; set; }
}
