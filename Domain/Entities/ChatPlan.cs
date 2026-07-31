using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace Domain.Entities;

[Table("ChatPlan")]
public class ChatPlan : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("RunId")]
    [ForeignKey("Run")]
    public Guid RunId { get; set; }
    public ChatRun? Run { get; set; }

    [Required]
    [Column("SessionId")]
    public Guid SessionId { get; set; }

    /// <summary>Tăng mỗi lần plan bị sửa. Dùng cho optimistic concurrency.</summary>
    public int Version { get; set; } = 1;

    [Required]
    [Column("Status", TypeName = "nvarchar(30)")]
    public string Status { get; set; } = ChatPlanStatus.Drafting;

    /// <summary>Danh sách bước, JSON array của PlanStepDto.</summary>
    [Required]
    [Column("Steps", TypeName = "nvarchar(max)")]
    public string Steps { get; set; } = "[]";

    /// <summary>Ai chỉnh sửa lần cuối: "ai" hoặc "user".</summary>
    [Column("LastEditedBy", TypeName = "nvarchar(20)")]
    public string LastEditedBy { get; set; } = "ai";

    public DateTime? ApprovedAt { get; set; }

    /// <summary>Dấu vân tay registry tool lúc plan sinh ra — revalidate lại giá trị này lúc Duyệt (Stage 17.8).</summary>
    [Column("ToolRegistryFingerprint", TypeName = "nvarchar(20)")]
    public string? ToolRegistryFingerprint { get; set; }
}
