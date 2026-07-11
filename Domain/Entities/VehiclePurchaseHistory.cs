using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("VehiclePurchaseHistory")]
public class VehiclePurchaseHistory : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("VehicleId")]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    [Column("UserId")]
    public Guid? UserId { get; set; }

    [Column("PurchaseDate")]
    public DateTimeOffset PurchaseDate { get; set; }

    [Column("InvoiceNumber", TypeName = "nvarchar(100)")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Column("Amount", TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [Column("SellerName", TypeName = "nvarchar(255)")]
    public string SellerName { get; set; } = string.Empty;

    [Column("Notes", TypeName = "nvarchar(1000)")]
    public string? Notes { get; set; }
}
