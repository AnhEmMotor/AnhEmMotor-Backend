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

    public ApplicationUser? AssignedUser { get; set; }

    [Column("CustomerTrackingToken")]
    public Guid? CustomerTrackingToken { get; set; }

    [Column("AssignedAt")]
    public DateTimeOffset? AssignedAt { get; set; }

    [Column("StartedAt")]
    public DateTimeOffset? StartedAt { get; set; }

    [Column("ClosedAt")]
    public DateTimeOffset? ClosedAt { get; set; }

    [Column("EmployeeRatingOfCustomer")]
    public int? EmployeeRatingOfCustomer { get; set; }

    [Column("EmployeeRatingComment", TypeName = "nvarchar(1000)")]
    public string? EmployeeRatingComment { get; set; }

    [Column("EmployeeRatedAt")]
    public DateTimeOffset? EmployeeRatedAt { get; set; }

    [Column("CustomerRatingOfEmployee")]
    public int? CustomerRatingOfEmployee { get; set; }

    [Column("CustomerRatingComment", TypeName = "nvarchar(1000)")]
    public string? CustomerRatingComment { get; set; }

    [Column("CustomerRatedAt")]
    public DateTimeOffset? CustomerRatedAt { get; set; }
}
