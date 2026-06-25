using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("NewsProduct")]
    public class NewsProduct : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("NewsId")]
        public int NewsId { get; set; }

        [ForeignKey("NewsId")]
        public News? News { get; set; }

        [Column("ProductVariantId")]
        public int ProductVariantId { get; set; }

        [ForeignKey("ProductVariantId")]
        public ProductVariant? ProductVariant { get; set; }

        [Column("ProductVariantColorId")]
        public int? ProductVariantColorId { get; set; }

        [ForeignKey("ProductVariantColorId")]
        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
