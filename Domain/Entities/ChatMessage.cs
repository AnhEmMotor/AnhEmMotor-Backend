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

    /// <summary>Danh sách tool (name+label) đã dùng để sinh ra tin nhắn này, dạng JSON — null nếu không gọi tool nào.</summary>
    [Column("ToolCallsJson", TypeName = "nvarchar(max)")]
    public string? ToolCallsJson { get; set; }
}
