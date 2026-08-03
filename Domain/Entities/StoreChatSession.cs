using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace Domain.Entities;

[Table("StoreChatSession")]
public class StoreChatSession : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("VisitorKey", TypeName = "nvarchar(64)")]
    public string VisitorKey { get; set; } = string.Empty;

    [Column("CustomerUserId")]
    [ForeignKey("CustomerUser")]
    public Guid? CustomerUserId { get; set; }

    public ApplicationUser? CustomerUser { get; set; }

    [Required]
    [Column("Mode", TypeName = "nvarchar(20)")]
    public string Mode { get; set; } = StoreChatMode.Ai;

    [Column("AssignedStaffId")]
    public Guid? AssignedStaffId { get; set; }

    [Column("ContactName", TypeName = "nvarchar(100)")]
    public string? ContactName { get; set; }

    [Column("ContactPhone", TypeName = "nvarchar(20)")]
    public string? ContactPhone { get; set; }

    /// <summary>Phiên trước khi khách bấm "Xoá cuộc trò chuyện" — mỗi lần xoá tạo phiên mới, liên kết
    /// lại phiên cũ để quản trị lần theo được, phiên cũ vẫn giữ nguyên lịch sử, không nhận tin mới nữa.</summary>
    [Column("PreviousSessionId")]
    public Guid? PreviousSessionId { get; set; }

    [Required]
    public DateTime LastMessageAt { get; set; }

    public ICollection<StoreChatMessage> Messages { get; set; } = [];
}
