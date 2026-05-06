using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

[Table("EmployeeProfile")]
public class EmployeeProfile : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;

    [Column(TypeName = "nvarchar(20)")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(255)")]
    public string Address { get; set; } = string.Empty;

    public DateTime ContractDate { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string BankName { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(50)")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(50)")]
    public string JobTitle { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    public ICollection<CommissionRecord> CommissionRecords { get; set; } = [];

    public ICollection<KPI> KPIs { get; set; } = [];
}
