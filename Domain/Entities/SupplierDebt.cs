using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierDebt")]
    public class SupplierDebt : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PurchaseInvoiceId")]
        [ForeignKey("PurchaseInvoice")]
        public int PurchaseInvoiceId { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column("PaidAmount", TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }

        public PurchaseInvoice? PurchaseInvoice { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
