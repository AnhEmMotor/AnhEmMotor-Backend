using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("SupportRequest")]
public class SupportRequest : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ContactId")]
    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    [Column("Subject", TypeName = "nvarchar(200)")]
    public string Subject { get; set; } = string.Empty;

    [Column("Category", TypeName = "nvarchar(50)")]
    public string Category { get; set; } = string.Empty;

    [Column("Email", TypeName = "nvarchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("OrderCode", TypeName = "nvarchar(50)")]
    public string? OrderCode { get; set; }

    [Column("Content", TypeName = "nvarchar(MAX)")]
    public string Content { get; set; } = string.Empty;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = SupportRequestStatus.New;

    [Column("AssignedUserId")]
    public Guid? AssignedUserId { get; set; }
}
