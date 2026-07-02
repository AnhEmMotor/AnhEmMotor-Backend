using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseInvoice")]
    public class PurchaseInvoice : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InvoiceNumber", TypeName = "nvarchar(50)")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Column("PurchaseRequestId")]
        [ForeignKey("PurchaseRequest")]
        public int? PurchaseRequestId { get; set; }

        public Domain.Entities.PurchaseRequest? PurchaseRequest { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        [Column("SupplierName", TypeName = "nvarchar(200)")]
        public string? SupplierName { get; set; }

        [Column("SupplierPhone", TypeName = "nvarchar(20)")]
        public string? SupplierPhone { get; set; }

        [Column("SupplierAddress", TypeName = "nvarchar(500)")]
        public string? SupplierAddress { get; set; }

        [Column("SupplierTaxCode", TypeName = "nvarchar(50)")]
        public string? SupplierTaxCode { get; set; }

        [Column("CustomerName", TypeName = "nvarchar(200)")]
        public string? CustomerName { get; set; }

        [Column("CustomerPhone", TypeName = "nvarchar(20)")]
        public string? CustomerPhone { get; set; }

        [Column("CustomerAddress", TypeName = "nvarchar(500)")]
        public string? CustomerAddress { get; set; }

        [Column("CustomerIdCard", TypeName = "nvarchar(30)")]
        public string? CustomerIdCard { get; set; }

        [Column("InvoiceDate")]
        public DateTimeOffset InvoiceDate { get; set; }

        [Column("DueDate")]
        public DateTimeOffset? DueDate { get; set; }

        [Column("Status", TypeName = "nvarchar(30)")]
        public string Status { get; set; } = "draft";

        [Column("SubTotal", TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column("TaxAmount", TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("PaymentMethod", TypeName = "nvarchar(30)")]
        public string? PaymentMethod { get; set; }

        [Column("PaymentStatus", TypeName = "nvarchar(30)")]
        public string? PaymentStatus { get; set; }

        [Column("PaidAt")]
        public DateTimeOffset? PaidAt { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("CreatedByUserId")]
        public Guid? CreatedByUserId { get; set; }

        public virtual ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = [];
    }
}
