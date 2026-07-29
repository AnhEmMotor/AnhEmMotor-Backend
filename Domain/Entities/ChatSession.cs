using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ChatSession")]
public class ChatSession : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("UserId")]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [Required]
    [Column("Title", TypeName = "nvarchar(255)")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Ngữ cảnh định tuyến tool giữa các lượt (Stage 20), JSON. Nhỏ, ghi lại sau mỗi run.</summary>
    [Required]
    [Column("RoutingContext", TypeName = "nvarchar(max)")]
    public string RoutingContext { get; set; } = "{}";

    public ICollection<ChatMessage> Messages { get; set; } = [];
}
