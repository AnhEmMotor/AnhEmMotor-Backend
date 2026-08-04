using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("StoreChatMessage")]
public class StoreChatMessage : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("SessionId")]
    [ForeignKey("Session")]
    public Guid SessionId { get; set; }

    public StoreChatSession Session { get; set; } = null!;

    [Required]
    [Column("Sender", TypeName = "nvarchar(20)")]
    public string Sender { get; set; } = string.Empty;

    [Required]
    [Column("Content", TypeName = "nvarchar(max)")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Payload card sản phẩm/biến thể (Stage 02), JSON — null nếu tin nhắn không kèm card.
    /// </summary>
    [Column("CardsJson", TypeName = "nvarchar(max)")]
    public string? CardsJson { get; set; }
}
