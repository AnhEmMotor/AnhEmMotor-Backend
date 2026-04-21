using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Vehicle")]
public class Vehicle : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("LeadId")]
    [ForeignKey("Lead")]
    public int LeadId { get; set; }

    public Lead Lead { get; set; } = null!;

    [Column("VinNumber", TypeName = "nvarchar(100)")]
    public string VinNumber { get; set; } = string.Empty;

    [Column("EngineNumber", TypeName = "nvarchar(100)")]
    public string EngineNumber { get; set; } = string.Empty;

    [Column("LicensePlate", TypeName = "nvarchar(50)")]
    public string LicensePlate { get; set; } = string.Empty;

    [Column("PurchaseDate")]
    public System.DateTimeOffset PurchaseDate { get; set; }

    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();
}
