using Domain.Constants.Order;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Vehicle")]
public class Vehicle : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("LeadId")]
    [ForeignKey("Lead")]
    public int? LeadId { get; set; }

    public Lead? Lead { get; set; }

    [Column("InventoryReceiptInfoId")]
    [ForeignKey("InventoryReceiptInfo")]
    public int? InventoryReceiptInfoId { get; set; }

    public InventoryReceiptInfo? InventoryReceiptInfo { get; set; }

    [Column("OutputInfoId")]
    [ForeignKey("OutputInfo")]
    public int? OutputInfoId { get; set; }

    public OutputInfo? OutputInfo { get; set; }

    public Product? Product { get; set; }

    [Column("ProductVariantId")]
    [ForeignKey("ProductVariant")]
    public int? ProductVariantId { get; set; }

    public ProductVariant? ProductVariant { get; set; }

    [Column("ProductVariantColorId")]
    [ForeignKey("ProductVariantColor")]
    public int? ProductVariantColorId { get; set; }

    public ProductVariantColor? ProductVariantColor { get; set; }

    [Column("VinNumber", TypeName = "nvarchar(100)")]
    public string VinNumber { get; set; } = string.Empty;

    [Column("EngineNumber", TypeName = "nvarchar(100)")]
    public string EngineNumber { get; set; } = string.Empty;

    [Column("LicensePlate", TypeName = "nvarchar(50)")]
    public string LicensePlate { get; set; } = string.Empty;

    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("Status", TypeName = "nvarchar(50)")]
    public string Status { get; set; } = VehicleStatus.Available;

    [Column("PurchaseDate")]
    public DateTimeOffset PurchaseDate { get; set; }

    [Column("ImportPrice", TypeName = "decimal(18, 2)")]
    public decimal ImportPrice { get; set; } = 0;

    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();

    public ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();
}
