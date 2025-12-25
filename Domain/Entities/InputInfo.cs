using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InputInfo")]
    public class InputInfo : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InputId")]
        [ForeignKey("InputReceipt")]
        public int InputId { get; set; }

        [Column("ProductId")]
        [ForeignKey("ProductVariant")]
        public int? ProductId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("InputPrice", TypeName = "decimal(18, 2)")]
        public decimal? InputPrice { get; set; }

        [Column("RemainingCount")]
        public int? RemainingCount { get; set; }

        [Column("ParentOutputInfoId")]
        [ForeignKey("ParentOutputInfo")]
        public int? ParentOutputInfoId { get; set; }

        public Input? InputReceipt { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }
    }
}