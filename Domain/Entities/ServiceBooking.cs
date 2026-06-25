using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ServiceBooking")]
public class ServiceBooking : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ServiceId")]
    [ForeignKey("Service")]
    public int ServiceId { get; set; }

    public Service Service { get; set; } = null!;

    [Column("VehicleId")]
    [ForeignKey("Vehicle")]
    public int? VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    [Column("CustomerId")]
    [ForeignKey("Customer")]
    public Guid? CustomerId { get; set; }

    public ApplicationUser? Customer { get; set; }

    [Column("TechnicianId")]
    [ForeignKey("Technician")]
    public int? TechnicianId { get; set; }

    public EmployeeProfile? Technician { get; set; }

    [Column("ScheduledDate")]
    public DateTimeOffset ScheduledDate { get; set; }

    [Column("EstimatedDurationMinutes")]
    public int? EstimatedDurationMinutes { get; set; }

    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = global::Domain.Enums.BookingServiceStatus.Pending.ToString();

    [Column("PaymentStatus", TypeName = "nvarchar(20)")]
    public string PaymentStatus { get; set; } = global::Domain.Enums.PaymentStatus.Unpaid.ToString();

    [Column("TotalAmount", TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column("DepositAmount", TypeName = "decimal(18,2)")]
    public decimal? DepositAmount { get; set; }

    [Column("Notes", TypeName = "nvarchar(MAX)")]
    public string? Notes { get; set; }

    [Column("CustomerNotes", TypeName = "nvarchar(MAX)")]
    public string? CustomerNotes { get; set; }

    [Column("TechnicianNotes", TypeName = "nvarchar(MAX)")]
    public string? TechnicianNotes { get; set; }

    [Column("CompletedDate")]
    public DateTimeOffset? CompletedDate { get; set; }

    [Column("CancelledDate")]
    public DateTimeOffset? CancelledDate { get; set; }

    [Column("CancelledReason", TypeName = "nvarchar(500)")]
    public string? CancelledReason { get; set; }

    [Column("Rating")]
    public int? Rating { get; set; }

    [Column("Review", TypeName = "nvarchar(MAX)")]
    public string? Review { get; set; }
}
