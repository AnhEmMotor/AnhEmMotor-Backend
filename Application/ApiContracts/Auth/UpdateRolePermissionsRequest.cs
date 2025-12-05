namespace Application.ApiContracts.Auth;

public class UpdateRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
    public string? Description { get; set; }
}