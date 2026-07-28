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
}
