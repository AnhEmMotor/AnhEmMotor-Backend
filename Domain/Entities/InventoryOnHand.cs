using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryOnHand")]
    public class InventoryOnHand : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("StockQty")]
        public int StockQty { get; set; }

        [Column("ImportedQty")]
        public int ImportedQty { get; set; }

        [Column("ExportedQty")]
        public int ExportedQty { get; set; }

        [Column("OrderedQty")]
        public int OrderedQty { get; set; }

        [Column("PickingQty")]
        public int PickingQty { get; set; }

        [Column("TestDriveQty")]
        public int TestDriveQty { get; set; }

        [NotMapped]
        public int RemainingQty => StockQty - OrderedQty;

        public ProductVariant? ProductVariant { get; set; }
        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
