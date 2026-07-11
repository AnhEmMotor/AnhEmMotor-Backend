using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("VehicleWarrantyHistory")]
public class VehicleWarrantyHistory : BaseEntity
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("VehicleId")]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    [Column("UserId")]
    public Guid? UserId { get; set; }

    [Column("StartDate")]
    public DateTimeOffset StartDate { get; set; }

    [Column("EndDate")]
    public DateTimeOffset? EndDate { get; set; }

    [Column("ProviderName", TypeName = "nvarchar(255)")]
    public string ProviderName { get; set; } = string.Empty;

    [Column("PolicyNumber", TypeName = "nvarchar(100)")]
    public string PolicyNumber { get; set; } = string.Empty;

    [Column("Description", TypeName = "nvarchar(1000)")]
    public string? Description { get; set; }

    [Column("Status", TypeName = "nvarchar(50)")]
    public string Status { get; set; } = string.Empty;

    [Column("CoverageAmount", TypeName = "decimal(18, 2)")]
    public decimal CoverageAmount { get; set; }
}
