using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductCollectionPhoto")]
    public class ProductCollectionPhoto: BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ImageUrl", TypeName = "nvarchar(100)")]
        public string? ImageUrl { get; set; }

        public ProductVariant? ProductVariant { get; set; } = null!;
    }
}