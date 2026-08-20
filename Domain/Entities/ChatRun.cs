using Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// Tin nhắn steering đang chờ được nạp vào agent, dạng JSON array.
    /// </summary>
    [Column("PendingSteering", TypeName = "nvarchar(max)")]
    public string PendingSteering { get; set; } = "[]";

    /// <summary>
    /// Dấu vân tay registry tool lúc run bắt đầu — dùng để revalidate khi resume (Stage 17.2/17.8).
    /// </summary>
    [Column("ToolRegistryFingerprint", TypeName = "nvarchar(20)")]
    public string? ToolRegistryFingerprint { get; set; }

    /// <summary>
    /// Tên model thật lấy từ response metadata của LLM, không phải từ config (Stage 17.10).
    /// </summary>
    [Column("ModelUsed", TypeName = "nvarchar(100)")]
    public string? ModelUsed { get; set; }

    public ICollection<ChatRunEvent> Events { get; set; } = [];
}
