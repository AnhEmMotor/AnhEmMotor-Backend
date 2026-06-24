using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryTransaction")]
    public class InventoryTransaction : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("TransactionType", TypeName = "nvarchar(30)")]
        public string TransactionType { get; set; } = "adjustment";

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("StockBefore")]
        public int StockBefore { get; set; }

        [Column("StockAfter")]
        public int StockAfter { get; set; }

        [Column("ReferenceType", TypeName = "nvarchar(30)")]
        public string? ReferenceType { get; set; }

        [Column("ReferenceId")]
        public int? ReferenceId { get; set; }

        [Column("Note", TypeName = "nvarchar(500)")]
        public string? Note { get; set; }

        [Column("PerformedBy")]
        [ForeignKey("PerformedByUser")]
        public Guid? PerformedBy { get; set; }

        [Column("PerformedAt")]
        public DateTimeOffset? PerformedAt { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ApplicationUser? PerformedByUser { get; set; }
    }
}
