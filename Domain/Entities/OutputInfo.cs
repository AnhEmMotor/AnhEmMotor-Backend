using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OutputInfo")]
    public class OutputInfo : BaseEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ProductVarientId")]
        [ForeignKey("ProductVariant")]
        public int? ProductVarientId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("OutputId")]
        [ForeignKey("OutputOrder")]
        public int OutputId { get; set; }

        [Column("Price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        [Column("CostPrice", TypeName = "decimal(18, 2)")]
        public decimal? CostPrice { get; set; }

        public Output? OutputOrder { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ICollection<InputInfo> Returns { get; set; } = [];
    }
}