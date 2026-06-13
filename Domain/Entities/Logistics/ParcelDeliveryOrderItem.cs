using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Logistics;

public class ParcelDeliveryOrderItem
{
    [Key]
    public int Id { get; set; }

    public int ParcelDeliveryOrderId { get; set; }

    [ForeignKey(nameof(ParcelDeliveryOrderId))]
    public ParcelDeliveryOrder? ParcelDeliveryOrder { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string Sku { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Required]
    public string ShelfLocation { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public bool IsPicked { get; set; }

    public bool IsRestricted { get; set; }

    public bool IsOutOfStock { get; set; }
}
