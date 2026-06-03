using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ServiceEvaluation")]
public class ServiceEvaluation : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ServiceBookingId")]
    [ForeignKey("ServiceBooking")]
    public int ServiceBookingId { get; set; }

    public ServiceBooking ServiceBooking { get; set; } = null!;

    [Column("ContactId")]
    [ForeignKey("Contact")]
    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;

    [Column("Criteria", TypeName = "nvarchar(30)")]
    public string Criteria { get; set; } = ""; // QualityOfCar | AttitudeOfService

    [Column("Rating")]
    public int Rating { get; set; }

    [Column("Review", TypeName = "nvarchar(MAX)")]
    public string Review { get; set; } = "";

    [Column("ProcessingStatus", TypeName = "nvarchar(30)")]
    public string ProcessingStatus { get; set; } = "Unprocessed"; // Unprocessed | Processed

    [Column("InternalNotes", TypeName = "nvarchar(MAX)")]
    public string? InternalNotes { get; set; }

    [Column("DirectReplyText", TypeName = "nvarchar(MAX)")]
    public string? DirectReplyText { get; set; }

    [Column("AdminRepliedById")]
    public int? AdminRepliedById { get; set; }

    [Column("ProcessedAt")]
    public DateTimeOffset? ProcessedAt { get; set; }
}

