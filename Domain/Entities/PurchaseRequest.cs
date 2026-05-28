using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseRequest")]
    public class PurchaseRequest : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

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

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<PurchaseRequestItem> PurchaseRequestItems { get; set; } = [];
    }
}
