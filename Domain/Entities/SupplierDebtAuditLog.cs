using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierDebtAuditLog")]
    public class SupplierDebtAuditLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int SupplierDebtId { get; set; }

        public string Action { get; set; } = string.Empty;

        public Guid? ChangedById { get; set; }

        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        public decimal? OldAmount { get; set; }
        public decimal? NewAmount { get; set; }

        public string? OldNotes { get; set; }
        public string? NewNotes { get; set; }

        public DateTimeOffset? OldPaymentDate { get; set; }
        public DateTimeOffset? NewPaymentDate { get; set; }

        [ForeignKey("SupplierDebtId")]
        public virtual SupplierDebt SupplierDebt { get; set; } = null!;

        [ForeignKey("ChangedById")]
        public virtual ApplicationUser? ChangedBy { get; set; }
    }
}
