using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

[Table("CommissionPolicy")]
public class CommissionPolicy : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(50)")]
    public string Type { get; set; } = "FixedAmount";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    public int? ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public ProductCategory? Category { get; set; }

    public Guid? EmployeeId { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string? TargetGroup { get; set; }

    public DateTimeOffset EffectiveDate { get; set; } = DateTimeOffset.UtcNow;

    [Column(TypeName = "nvarchar(500)")]
    public string? Notes { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public string? Unit { get; set; }

    public bool IsActive { get; set; } = true;
}
