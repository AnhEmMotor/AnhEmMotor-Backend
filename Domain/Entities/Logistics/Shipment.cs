using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Logistics;

public class Shipment : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public Domain.Enums.ParcelDeliveryStatus Status { get; set; } = Domain.Enums.ParcelDeliveryStatus.Shipping;

    [Required]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string Carrier { get; set; } = "Giao Hàng Tiết Kiệm";

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    public decimal CodAmount { get; set; }

    public decimal ShippingCost { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }

    public string OriginAddress { get; set; } = string.Empty;

    public string DestinationAddress { get; set; } = string.Empty;

    public double? OriginLatitude { get; set; }

    public double? OriginLongitude { get; set; }

    public double? DestinationLatitude { get; set; }

    public double? DestinationLongitude { get; set; }

    [Required]
    public string Type { get; set; } = Constants.Logistics.ShipmentType.OrderDelivery;

    public int? OutputId { get; set; }

    [ForeignKey("OutputId")]
    public Output? Output { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = [];
}
