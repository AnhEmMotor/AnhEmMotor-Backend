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
    
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
