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

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int? ProductVariantId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("InputPrice", TypeName = "decimal(18, 2)")]
        public decimal? InputPrice { get; set; }

        [Column("RemainingCount")]
        public int? RemainingCount { get; set; }

        [Column("ParentOutputInfoId")]
        [ForeignKey("ParentOutputInfo")]
        public int? ParentOutputInfoId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        public Input? InputReceipt { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ProductVariantColor? ProductVariantColor { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
