using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseOrder")]
    public class PurchaseOrder : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PurchaseRequestId")]
        [ForeignKey("PurchaseRequest")]
        public int? PurchaseRequestId { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Column("Status", TypeName = "varchar(30)")]
        public string Status { get; set; } = "draft";

        [Column("OrderDate")]
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;

        [Column("Note", TypeName = "nvarchar(MAX)")]
        public string? Note { get; set; }

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("SentBy")]
        [ForeignKey("SentByUser")]
        public Guid? SentBy { get; set; }

        [Column("ApprovedBy")]
        [ForeignKey("ApprovedByUser")]
        public Guid? ApprovedBy { get; set; }

        [Column("RejectedBy")]
        [ForeignKey("RejectedByUser")]
        public Guid? RejectedBy { get; set; }

        // Navigation Properties
        public PurchaseRequest? PurchaseRequest { get; set; }
        public Supplier? Supplier { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }
        public ApplicationUser? SentByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }

        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
        public ICollection<InventoryReceipt> InventoryReceipts { get; set; } = [];
        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = [];
    }
}
