using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("MaintenanceHistory")]
public class MaintenanceHistory : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("VehicleId")]
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    [Required]
    [Column("MaintenanceNumber", TypeName = "nvarchar(50)")]
    public string MaintenanceNumber { get; set; } = string.Empty;

    [Column("MaintenanceDate")]
    public DateTimeOffset MaintenanceDate { get; set; }

    [Column("Mileage")]
    public int Mileage { get; set; } = 0;

    [Column("Description", TypeName = "nvarchar(MAX)")]
    public string Description { get; set; } = string.Empty;

    [Column("TechnicianId")]
    public int? TechnicianId { get; set; }

    [ForeignKey("TechnicianId")]
    public EmployeeProfile? Technician { get; set; }

    [Column("LaborCost", TypeName = "decimal(18, 2)")]
    public decimal LaborCost { get; set; } = 0;

    [Column("PartsCost", TypeName = "decimal(18, 2)")]
    public decimal PartsCost { get; set; } = 0;

    [Column("TotalCost", TypeName = "decimal(18, 2)")]
    public decimal TotalCost { get; set; } = 0;

    [Column("NextMaintenanceDate")]
    public DateTimeOffset? NextMaintenanceDate { get; set; }

    [Column("NextMaintenanceOdo")]
    public int? NextMaintenanceOdo { get; set; }

    [Column("PartsJson", TypeName = "nvarchar(MAX)")]
    public string? PartsJson { get; set; }
}
