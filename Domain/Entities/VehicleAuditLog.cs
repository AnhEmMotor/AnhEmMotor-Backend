using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("VehicleAuditLog")]
    public class VehicleAuditLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int VehicleId { get; set; }

        public string Action { get; set; } = string.Empty;

        public Guid? ChangedById { get; set; }

        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? OldVinNumber { get; set; }
        public string? NewVinNumber { get; set; }

        public string? OldEngineNumber { get; set; }
        public string? NewEngineNumber { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;

        [ForeignKey("ChangedById")]
        public virtual ApplicationUser? ChangedBy { get; set; }
    }
}
