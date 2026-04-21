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

    [Column("MaintenanceDate")]
    public System.DateTimeOffset MaintenanceDate { get; set; }

    [Column("Mileage")]
    public int Mileage { get; set; } = 0;

    [Column("Description", TypeName = "nvarchar(MAX)")]
    public string Description { get; set; } = string.Empty;
}
