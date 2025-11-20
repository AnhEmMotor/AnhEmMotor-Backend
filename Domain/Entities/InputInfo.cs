using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InputInfo")]
    public class InputInfo
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InputId")]
        [ForeignKey("InputReceipt")]
        public int? InputId { get; set; }

        [Column("ProductId")]
        [ForeignKey("ProductVariant")]
        public int? ProductId { get; set; }

        [Column("Count")]
        public short? Count { get; set; }

        [Column("InputPrice")]
        public long? InputPrice { get; set; }

        [Column("RemainingCount")]
        public long? RemainingCount { get; set; }

        public Input? InputReceipt { get; set; }

        public ProductVariant? ProductVariant { get; set; }
    }
}