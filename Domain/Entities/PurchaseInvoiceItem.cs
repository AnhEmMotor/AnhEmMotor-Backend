using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseInvoiceItem")]
    public class PurchaseInvoiceItem : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PurchaseInvoiceId")]
        [ForeignKey("PurchaseInvoice")]
        public int PurchaseInvoiceId { get; set; }

        [Column("PurchaseOrderItemId")]
        [ForeignKey("PurchaseOrderItem")]
        public int? PurchaseOrderItemId { get; set; }

        [Column("InventoryReceiptItemId")]
        [ForeignKey("InventoryReceiptInfo")]
        public int? InventoryReceiptItemId { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("InvoicedQuantity")]
        public int InvoicedQuantity { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column("TaxRate", TypeName = "decimal(5, 2)")]
        public decimal TaxRate { get; set; } = 0;

        // Navigation Properties
        public PurchaseInvoice? PurchaseInvoice { get; set; }
        public PurchaseOrderItem? PurchaseOrderItem { get; set; }
        public InventoryReceiptInfo? InventoryReceiptInfo { get; set; }
        public ProductVariant? ProductVariant { get; set; }
        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
