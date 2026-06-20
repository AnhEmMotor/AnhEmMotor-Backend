using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseRequestItem")]
    public class PurchaseRequestItem : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PurchaseRequestId")]
        [ForeignKey("PurchaseRequest")]
        public int PurchaseRequestId { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        public PurchaseRequest? PurchaseRequest { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ProductVariantColor? ProductVariantColor { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public ICollection<InventoryReceiptInfo> InventoryReceiptInfos { get; set; } = [];
    }
}
