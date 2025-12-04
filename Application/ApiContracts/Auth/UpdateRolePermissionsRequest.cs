namespace Application.ApiContracts.Auth;

/// <summary>
/// DTO cho cập nhật quyền của vai trò
/// </summary>
public class UpdateRolePermissionsRequest
{
    /// <summary>
    /// Danh sách các permissions
    /// </summary>
    public List<string> Permissions { get; set; } = [];
}
