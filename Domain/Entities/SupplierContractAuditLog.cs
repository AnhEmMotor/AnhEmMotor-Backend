using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("SupplierContractAuditLog")]
public class SupplierContractAuditLog : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SupplierContractId { get; set; }

    [ForeignKey("SupplierContractId")]
    public SupplierContract? SupplierContract { get; set; }

    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(200)]
    public string? OldValue { get; set; }

    [MaxLength(200)]
    public string? NewValue { get; set; }
}
