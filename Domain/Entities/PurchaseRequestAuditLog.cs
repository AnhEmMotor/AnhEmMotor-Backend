using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("PurchaseRequestAuditLog")]
    public class PurchaseRequestAuditLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int PurchaseRequestId { get; set; }

        public string Action { get; set; } = string.Empty;

        public Guid? ChangedById { get; set; }

        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? OldStatusId { get; set; }

        public string? NewStatusId { get; set; }

        public string? OldNotes { get; set; }

        public string? NewNotes { get; set; }

        [ForeignKey("PurchaseRequestId")]
        public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

        [ForeignKey("ChangedById")]
        public virtual ApplicationUser? ChangedBy { get; set; }
    }
}
