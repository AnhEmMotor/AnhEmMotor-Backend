using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ChatMessage")]
public class ChatMessage : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("SessionId")]
    [ForeignKey("Session")]
    public Guid SessionId { get; set; }

    public ChatSession? Session { get; set; }

    [Required]
    [Column("Role", TypeName = "nvarchar(50)")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [Column("Message", TypeName = "nvarchar(max)")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Tin nhắn được gửi khi run đang chạy (steering).</summary>
    public bool IsSteering { get; set; }

    /// <summary>Run mà tin nhắn này gắn vào (null với tin nhắn khởi tạo run).</summary>
    [Column("RunId")]
    [ForeignKey("Run")]
    public Guid? RunId { get; set; }
    public ChatRun? Run { get; set; }

    /// <summary>Các bước suy nghĩ (thinking) + gọi tool đã dùng để sinh ra tin nhắn này, dạng JSON
    /// (ChatReasoningStepDto[]) — null nếu không có bước nào.</summary>
    [Column("ReasoningStepsJson", TypeName = "nvarchar(max)")]
    public string? ReasoningStepsJson { get; set; }

    /// <summary>Thời gian (giây) từ lúc đoạn suy nghĩ này bắt đầu đến lúc hoàn tất — null với tin
    /// nhắn không phải AI hoặc tin nhắn cũ trước khi cột này tồn tại.</summary>
    public double? ReasoningElapsedSeconds { get; set; }
}
