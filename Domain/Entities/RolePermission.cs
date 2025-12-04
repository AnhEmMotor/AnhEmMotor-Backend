namespace Domain.Entities;

/// <summary>
/// Bảng trung gian giữa Role và Permission
/// </summary>
public class RolePermission
{
    /// <summary>
    /// ID của vai trò
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Vai trò
    /// </summary>
    public virtual ApplicationRole? Role { get; set; }

    /// <summary>
    /// ID của quyền
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Quyền
    /// </summary>
    public virtual Permission? Permission { get; set; }
}
