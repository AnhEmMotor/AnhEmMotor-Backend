using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Logistics;

public class ShipmentItem : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int ShipmentId { get; set; }

    [ForeignKey("ShipmentId")]
    public Shipment? Shipment { get; set; }

    public int? ProductVariantId { get; set; }

    [ForeignKey("ProductVariantId")]
    public ProductVariant? ProductVariant { get; set; }

    public int? ProductVariantColorId { get; set; }

    [ForeignKey("ProductVariantColorId")]
    public ProductVariantColor? ProductVariantColor { get; set; }

    public int Quantity { get; set; }
}
