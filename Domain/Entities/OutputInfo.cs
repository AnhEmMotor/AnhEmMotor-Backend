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

        [Column("ProductId")]
        [ForeignKey("ProductVariant")]
        public int? ProductId { get; set; }

        [Column("Count")]
        public short? Count { get; set; }

        [Column("OutputId")]
        [ForeignKey("OutputOrder")]
        public int OutputId { get; set; }

        [Column("Price")]
        public long? Price { get; set; }

        [Column("CostPrice")]
        public long? CostPrice { get; set; }

        public Output? OutputOrder { get; set; }

        public ProductVariant? ProductVariant { get; set; }
    }
}