using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace Domain.Entities;

[Table("ChatRun")]
public class ChatRun : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("SessionId")]
    [ForeignKey("Session")]
    public Guid SessionId { get; set; }
    public ChatSession? Session { get; set; }

    [Required]
    [Column("Status", TypeName = "nvarchar(30)")]
    public string Status { get; set; } = ChatRunStatus.Pending;

    [Required]
    [Column("UserMessage", TypeName = "nvarchar(max)")]
    public string UserMessage { get; set; } = string.Empty;

    [Column("PartialOutput", TypeName = "nvarchar(max)")]
    public string PartialOutput { get; set; } = string.Empty;

    public long LastSeq { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [Column("ErrorCode", TypeName = "nvarchar(100)")]
    public string? ErrorCode { get; set; }

    [Column("OwnerInstanceId", TypeName = "nvarchar(100)")]
    public string? OwnerInstanceId { get; set; }

    public DateTime? HeartbeatAt { get; set; }

    public ICollection<ChatRunEvent> Events { get; set; } = [];
}
