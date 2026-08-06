using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ChatRunEvent")]
public class ChatRunEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("RunId")]
    [ForeignKey("Run")]
    public Guid RunId { get; set; }

    public ChatRun? Run { get; set; }

    [Required]
    public long Seq { get; set; }

    [Required]
    [Column("Type", TypeName = "nvarchar(40)")]
    public string Type { get; set; } = string.Empty;

    [Column("Payload", TypeName = "nvarchar(max)")]
    public string Payload { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
