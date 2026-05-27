using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductVariantColor")]
    public class ProductVariantColor : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductVariantId")]
        public int ProductVariantId { get; set; }

        [Column("ColorName", TypeName = "nvarchar(500)")]
        public string? ColorName { get; set; }

        [Column("ColorCode", TypeName = "nvarchar(200)")]
        public string? ColorCode { get; set; }

        [Column("CoverImageUrl", TypeName = "nvarchar(1000)")]
        public string? CoverImageUrl { get; set; }

        public ProductVariant? ProductVariant { get; set; }
    }
}
