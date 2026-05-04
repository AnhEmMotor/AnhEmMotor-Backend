using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Contact")]
public class Contact : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("FullName", TypeName = "nvarchar(100)")]
    public string FullName { get; set; } = string.Empty;

    [Column("Email", TypeName = "nvarchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("PhoneNumber", TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("Subject", TypeName = "nvarchar(200)")]
    public string Subject { get; set; } = string.Empty;

    [Column("Message", TypeName = "nvarchar(MAX)")]
    public string Message { get; set; } = string.Empty;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = "Pending";

    [Column("InternalNote", TypeName = "nvarchar(MAX)")]
    public string? InternalNote { get; set; }

    [Column("Rating")]
    public int? Rating { get; set; }

    public ICollection<ContactReply> Replies { get; set; } = new List<ContactReply>();
}
