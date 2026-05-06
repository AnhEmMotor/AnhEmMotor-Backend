using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

[Table("KPI")]
public class KPI : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeProfileId { get; set; }

    [ForeignKey("EmployeeProfileId")]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string MetricName { get; set; } = string.Empty; // e.g. "Vehicles Sold"

    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualValue { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    [Column(TypeName = "nvarchar(MAX)")]
    public string? Description { get; set; }
}
