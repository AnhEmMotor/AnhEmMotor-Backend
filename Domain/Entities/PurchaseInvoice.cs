using System;
using System.Collections.Generic;
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

        [Column("PurchaseOrderId")]
        [ForeignKey("PurchaseOrder")]
        public int? PurchaseOrderId { get; set; }

        [Column("InvoiceNumber", TypeName = "varchar(50)")]
        public string? InvoiceNumber { get; set; }

        [Column("InvoiceDate")]
        public DateTimeOffset InvoiceDate { get; set; } = DateTimeOffset.UtcNow;

        [Column("DueDate")]
        public DateTimeOffset? DueDate { get; set; }

        [Column("Status", TypeName = "varchar(30)")]
        public string Status { get; set; } = "draft";

        [Column("Note", TypeName = "nvarchar(MAX)")]
        public string? Note { get; set; }

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("ApprovedBy")]
        [ForeignKey("ApprovedByUser")]
        public Guid? ApprovedBy { get; set; }

        // Navigation Properties
        public PurchaseOrder? PurchaseOrder { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = [];
        public ICollection<SupplierDebt> SupplierDebts { get; set; } = [];
    }
}
