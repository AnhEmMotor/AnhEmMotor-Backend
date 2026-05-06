using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ContactReply")]
public class ContactReply : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ContactId")]
    [ForeignKey("Contact")]
    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    [Column("Message", TypeName = "nvarchar(MAX)")]
    public string Message { get; set; } = string.Empty;

    [Column("RepliedById")]
    [ForeignKey("RepliedBy")]
    public Guid RepliedById { get; set; }

    public ApplicationUser RepliedBy { get; set; } = null!;
}
