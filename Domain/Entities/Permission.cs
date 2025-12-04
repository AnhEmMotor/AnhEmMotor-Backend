using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Quyền hạn trong hệ thống
/// </summary>
public class Permission
{
    /// <summary>
    /// ID của quyền
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên quyền (ví dụ: Permissions.Products.View)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả ngắn của quyền (dùng cho i18n) - không lưu vào database
    /// </summary>
    [NotMapped]
    public string? Description { get; set; }

    /// <summary>
    /// Các vai trò có quyền này
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
