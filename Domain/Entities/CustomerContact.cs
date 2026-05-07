using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("CustomerContact")]
    public class CustomerContact : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("FullName", TypeName = "nvarchar(150)")]
        public string FullName { get; set; } = string.Empty;

        [Column("Phone", TypeName = "nvarchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Column("Email", TypeName = "nvarchar(150)")]
        public string? Email { get; set; }

        [Column("Subject", TypeName = "nvarchar(200)")]
        public string? Subject { get; set; }

        [Column("Message", TypeName = "nvarchar(MAX)")]
        public string Message { get; set; } = string.Empty;

        [Column("Status", TypeName = "nvarchar(30)")]
        public string Status { get; set; } = "new";

        [Column("Source", TypeName = "nvarchar(30)")]
        public string? Source { get; set; }

        [Column("ProcessedAt")]
        public DateTimeOffset? ProcessedAt { get; set; }

        [Column("ProcessedBy")]
        [ForeignKey("ProcessedByUser")]
        public Guid? ProcessedBy { get; set; }

        [Column("RepliedAt")]
        public DateTimeOffset? RepliedAt { get; set; }

        [Column("InternalNote", TypeName = "nvarchar(MAX)")]
        public string? InternalNote { get; set; }

        public ApplicationUser? ProcessedByUser { get; set; }

        public ICollection<CustomerContactReply> Replies { get; set; } = [];
    }
}
