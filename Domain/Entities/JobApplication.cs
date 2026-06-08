using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

[Table("JobApplication")]
public class JobApplication : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ContactId")]
    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    [Column("FullName", TypeName = "nvarchar(100)")]
    public string FullName { get; set; } = string.Empty;

    [Column("Email", TypeName = "nvarchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Column("PhoneNumber", TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("AppliedPosition", TypeName = "nvarchar(100)")]
    public string AppliedPosition { get; set; } = string.Empty;

    [Column("CvFileUrl", TypeName = "nvarchar(500)")]
    public string CvFileUrl { get; set; } = string.Empty;

    [Column("CoverLetter", TypeName = "nvarchar(MAX)")]
    public string? CoverLetter { get; set; }

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = AppStatus.New;
}
