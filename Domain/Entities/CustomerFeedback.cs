using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("CustomerFeedback")]
public class CustomerFeedback : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ContactId")]
    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    [Column("Rating")]
    public int Rating { get; set; }

    [Column("FeedbackArea", TypeName = "nvarchar(50)")]
    public string FeedbackArea { get; set; } = string.Empty;

    [Column("CustomerName", TypeName = "nvarchar(100)")]
    public string CustomerName { get; set; } = string.Empty;

    [Column("PhoneNumber", TypeName = "nvarchar(20)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("Content", TypeName = "nvarchar(MAX)")]
    public string Content { get; set; } = string.Empty;

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = FeedbackStatus.Pending;
}
