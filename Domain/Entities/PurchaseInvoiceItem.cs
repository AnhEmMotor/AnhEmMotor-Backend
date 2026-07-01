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

        public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;

        [Column("PurchaseRequestItemId")]
        public int? PurchaseRequestItemId { get; set; }

        [Column("ProductVariantId")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        public int? ProductVariantColorId { get; set; }

        [Column("ProductName", TypeName = "nvarchar(200)")]
        public string? ProductName { get; set; }

        [Column("VariantName", TypeName = "nvarchar(100)")]
        public string? VariantName { get; set; }

        [Column("ColorName", TypeName = "nvarchar(50)")]
        public string? ColorName { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column("TaxRate", TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; }

        [Column("TaxAmount", TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }
}
