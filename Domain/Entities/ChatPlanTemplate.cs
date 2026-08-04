using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ChatPlanTemplate")]
public class ChatPlanTemplate : BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Câu hỏi đại diện đã sinh ra template này (để đối chiếu và debug).
    /// </summary>
    [Required]
    [Column("CanonicalQuestion", TypeName = "nvarchar(500)")]
    public string CanonicalQuestion { get; set; } = string.Empty;

    /// <summary>
    /// Hash chuẩn hoá của ý định — khoá tra cứu chính xác.
    /// </summary>
    [Required]
    [Column("IntentHash", TypeName = "nvarchar(64)")]
    public string IntentHash { get; set; } = string.Empty;

    /// <summary>
    /// Các bước đã tham số hoá (slot thay cho giá trị cụ thể), JSON.
    /// </summary>
    [Required]
    [Column("StepsTemplate", TypeName = "nvarchar(max)")]
    public string StepsTemplate { get; set; } = "[]";

    /// <summary>
    /// Danh sách slot cần điền, JSON. Ví dụ: from_date, to_date, category.
    /// </summary>
    [Required]
    [Column("Slots", TypeName = "nvarchar(max)")]
    public string Slots { get; set; } = "[]";

    /// <summary>
    /// Tool mà template cần — dùng để vô hiệu khi tool bị gỡ (Stage 17).
    /// </summary>
    [Required]
    [Column("RequiredTools", TypeName = "nvarchar(max)")]
    public string RequiredTools { get; set; } = "[]";

    /// <summary>
    /// Permission tối thiểu để dùng template này.
    /// </summary>
    [Required]
    [Column("RequiredPermissions", TypeName = "nvarchar(max)")]
    public string RequiredPermissions { get; set; } = "[]";

    /// <summary>
    /// Fingerprint registry lúc template được tạo (Stage 17.2).
    /// </summary>
    [Column("ToolRegistryFingerprint", TypeName = "nvarchar(32)")]
    public string? ToolRegistryFingerprint { get; set; }

    /// <summary>
    /// Module định tuyến — dùng để lọc template theo module, không lẫn giữa các nghiệp vụ.
    /// </summary>
    [Required]
    [Column("Module", TypeName = "nvarchar(50)")]
    public string Module { get; set; } = string.Empty;

    public int UseCount { get; set; }

    public int SuccessCount { get; set; }

    /// <summary>
    /// User sửa bao nhiêu lần — template chưa tốt nếu tỉ lệ cao (19.6).
    /// </summary>
    public int UserEditCount { get; set; }

    /// <summary>
    /// User huỷ bao nhiêu lần — không học từ những lần này (19.7).
    /// </summary>
    public int RejectCount { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// active | stale | disabled (19.6).
    /// </summary>
    [Required]
    [Column("Status", TypeName = "nvarchar(20)")]
    public string Status { get; set; } = "active";
}
