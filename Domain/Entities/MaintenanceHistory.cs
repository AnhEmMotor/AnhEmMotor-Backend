using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class MaintenanceHistory : BaseEntity
{
    public int Id { get; set; }
    public string MaintenanceNumber { get; set; } = string.Empty;

    [Column("VehicleId")]
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public DateTimeOffset MaintenanceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public int? TechnicianId { get; set; }
    public decimal PartsCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? PartsJson { get; set; }
    public DateTimeOffset? NextMaintenanceDate { get; set; }
    public int? NextMaintenanceOdo { get; set; }
}
