using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Logistics;

public class ParcelDeliveryOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string Carrier { get; set; } = string.Empty;

    public ParcelDeliveryStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpectedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public decimal CodAmount { get; set; }

    public decimal ShippingCost { get; set; }

    public DateTime? InspectedAt { get; set; }

    public string? ReturnReason { get; set; }

    public string? BoxCondition { get; set; }

    public string? ProductCondition { get; set; }

    public string? ReturnProofImage { get; set; }

    public string? ReturnInternalNote { get; set; }

    public string? ReturnAction { get; set; }

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    public string CustomerAddress { get; set; } = string.Empty;

    public string OriginalOrderCode { get; set; } = string.Empty;

    public ICollection<ParcelDeliveryOrderItem> Items { get; set; } = [];

    public ParcelDeliveryOrder()
    {
    }
}

