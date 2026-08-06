using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ChatFeedback")]
public class ChatFeedback : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Column("ChatRunId")]
    [ForeignKey("ChatRun")]
    public Guid ChatRunId { get; set; }

    public ChatRun? ChatRun { get; set; }

    [Column("Comment", TypeName = "nvarchar(max)")]
    public string? Comment { get; set; }

    [Required]
    [Column("ReportedBy")]
    [ForeignKey("ReportedByUser")]
    public Guid ReportedBy { get; set; }

    public ApplicationUser? ReportedByUser { get; set; }
}
