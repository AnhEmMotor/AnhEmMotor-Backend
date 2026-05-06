using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

[Table("Payroll")]
public class Payroll : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmployeeProfileId { get; set; }

    [ForeignKey("EmployeeProfileId")]
    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    public int Month { get; set; }

    public int Year { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCommission { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Penalty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalSalary { get; set; }

    public bool IsApproved { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}
