namespace Application.ApiContracts.User;

/// <summary>
/// DTO gán roles cho user
/// </summary>
public class AssignRolesRequest
{
    /// <summary>
    /// Danh sách role names
    /// </summary>
    public List<string> RoleNames { get; set; } = [];
}
