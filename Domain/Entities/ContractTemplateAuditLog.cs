using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Audit log cho việc thay đổi ContractTemplate (versioning / activate / deactivate / update content / soft delete).
/// </summary>
[Table("ContractTemplateAuditLog")]
public class ContractTemplateAuditLog : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ContractTemplateId { get; set; }

    [ForeignKey("ContractTemplateId")]
    public ContractTemplate? ContractTemplate { get; set; }

    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(2000)]
    public string? OldValue { get; set; }

    [MaxLength(2000)]
    public string? NewValue { get; set; }
}

