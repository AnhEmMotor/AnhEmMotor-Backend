using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants.Workshop;

namespace Domain.Entities;

[Table("WorkshopTickets")]
public class WorkshopTicket : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("VehicleId")]
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    [Column("TechnicianId")]
    [ForeignKey("Technician")]
    public Guid? TechnicianId { get; set; }
    public ApplicationUser? Technician { get; set; }

    [Column("Status")]
    public WorkshopTicketStatus Status { get; set; } = WorkshopTicketStatus.Pending;

    [Column("StartTime")]
    public DateTimeOffset? StartTime { get; set; }

    [Column("EndTime")]
    public DateTimeOffset? EndTime { get; set; }

    [Column("ExpectedCompletionTime")]
    public DateTimeOffset? ExpectedCompletionTime { get; set; }

    [Column("Description", TypeName = "nvarchar(MAX)")]
    public string? Description { get; set; }

    [Column("Notes", TypeName = "nvarchar(MAX)")]
    public string? Notes { get; set; }

    [Column("OutputId")]
    [ForeignKey("OutputOrder")]
    public int? OutputId { get; set; }
    public Output? OutputOrder { get; set; }

    public ICollection<WorkshopTicketItem> TicketItems { get; set; } = [];
}
