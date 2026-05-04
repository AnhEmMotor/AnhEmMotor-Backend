using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Lead")]
public class Lead : BaseEntity
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

    [Column("Score")]
    public int Score { get; set; } = 0;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = "New";

    [Column("Source", TypeName = "nvarchar(50)")]
    public string Source { get; set; } = "WebStore";

    [Column("Address", TypeName = "nvarchar(500)")]
    public string Address { get; set; } = string.Empty;

    [Column("Tier", TypeName = "nvarchar(50)")]
    public string Tier { get; set; } = "Thành viên mới";

    [Column("Points")]
    public int Points { get; set; } = 0;

    public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
}
