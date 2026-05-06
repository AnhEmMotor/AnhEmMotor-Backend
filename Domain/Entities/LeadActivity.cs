using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("LeadActivity")]
public class LeadActivity : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("LeadId")]
    [ForeignKey("Lead")]
    public int LeadId { get; set; }

    public Lead Lead { get; set; } = null!;

    [Column("ActivityType", TypeName = "nvarchar(50)")]
    public string ActivityType { get; set; } = "Note"; // Note, Booking, Contact, Call, AI_Query

    [Column("Description", TypeName = "nvarchar(MAX)")]
    public string Description { get; set; } = string.Empty;

}
