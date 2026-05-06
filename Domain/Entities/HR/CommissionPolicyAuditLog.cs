using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR;

/// <summary>
/// Audit log ghi lại mọi thay đổi về chính sách hoa hồng để đảm bảo tính minh bạch và giải quyết tranh chấp tiền
/// thưởng.
/// </summary>
[Table("CommissionPolicyAuditLog")]
public class CommissionPolicyAuditLog : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int PolicyId { get; set; }

    [ForeignKey("PolicyId")]
    public CommissionPolicy Policy { get; set; } = null!;

    /// <summary>
    /// Hành động: Created, Updated, Archived
    /// </summary>
    [Column(TypeName = "nvarchar(20)")]
    public string Action { get; set; } = "Updated";

    /// <summary>
    /// Tên người thực hiện thay đổi
    /// </summary>
    [Column(TypeName = "nvarchar(200)")]
    public string ChangedByName { get; set; } = string.Empty;

    public Guid ChangedByUserId { get; set; }

    /// <summary>
    /// Giá trị cũ (JSON snapshot)
    /// </summary>
    [Column(TypeName = "nvarchar(MAX)")]
    public string? OldValueSnapshot { get; set; }

    /// <summary>
    /// Giá trị mới (JSON snapshot)
    /// </summary>
    [Column(TypeName = "nvarchar(MAX)")]
    public string? NewValueSnapshot { get; set; }

    /// <summary>
    /// Mô tả thay đổi dễ đọc: "Tăng thưởng Winner X từ 500k lên 700k"
    /// </summary>
    [Column(TypeName = "nvarchar(500)")]
    public string? Description { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
