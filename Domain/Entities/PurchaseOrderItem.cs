using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseOrderItem")]
    public class PurchaseOrderItem : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PurchaseOrderId")]
        [ForeignKey("PurchaseOrder")]
        public int PurchaseOrderId { get; set; }

        [Column("PurchaseRequestItemId")]
        [ForeignKey("PurchaseRequestItem")]
        public int? PurchaseRequestItemId { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("OrderedQuantity")]
        public int OrderedQuantity { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column("QuotationProductRowId")]
        [ForeignKey("QuotationProductRow")]
        public int? QuotationProductRowId { get; set; }

        // Navigation Properties
        public PurchaseOrder? PurchaseOrder { get; set; }
        public PurchaseRequestItem? PurchaseRequestItem { get; set; }
        public ProductVariant? ProductVariant { get; set; }
        public ProductVariantColor? ProductVariantColor { get; set; }
        public QuotationProductRow? QuotationProductRow { get; set; }
    }
}
