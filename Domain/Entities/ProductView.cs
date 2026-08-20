using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ProductView")]
public class ProductView : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("ProductId")]
    [ForeignKey("Product")]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Column("VariantId")]
    [ForeignKey("Variant")]
    public int? VariantId { get; set; }

    public ProductVariant? Variant { get; set; }

    [Column("VariantColorId")]
    [ForeignKey("VariantColor")]
    public int? VariantColorId { get; set; }

    public ProductVariantColor? VariantColor { get; set; }

    [Column("CustomerUserId")]
    [ForeignKey("CustomerUser")]
    public Guid? CustomerUserId { get; set; }

    public ApplicationUser? CustomerUser { get; set; }

    [Column("VisitorKey", TypeName = "nvarchar(64)")]
    public string? VisitorKey { get; set; }

    [Required]
    public int DwellTimeMs { get; set; }

    [Required]
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
