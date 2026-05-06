using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("BannerAuditLog")]
    public class BannerAuditLog : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("BannerId")]
        public int BannerId { get; set; }

        [Column("Action", TypeName = "nvarchar(50)")]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Pause, Resume

        [Column("ChangedBy", TypeName = "nvarchar(255)")]
        public string ChangedBy { get; set; } = string.Empty;

        [Column("Details", TypeName = "nvarchar(max)")]
        public string? Details { get; set; }
        
        [ForeignKey("BannerId")]
        public virtual Banner Banner { get; set; } = null!;
    }
}
