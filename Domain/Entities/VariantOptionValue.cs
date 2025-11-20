using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("VariantOptionValue")]
    public class VariantOptionValue
    {
        [Key]
        [Column("VariantId")]
        [ForeignKey("ProductVariant")]
        public int VariantId { get; set; }

        [Key]
        [Column("OptionValueId")]
        [ForeignKey("OptionValue")]
        public int? OptionValueId { get; set; }

        public ProductVariant? ProductVariant { get; set; } = null!;

        public OptionValue? OptionValue { get; set; } = null!;
    }
}