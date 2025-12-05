using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Vai trò trong hệ thống
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Mô tả của vai trò
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Các quyền của vai trò này
    /// </summary>
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
