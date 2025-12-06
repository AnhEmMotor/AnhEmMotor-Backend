namespace Application.ApiContracts.Permission.Requests;

public class UpdateRoleRequest
{
    public List<string> Permissions { get; set; } = [];
    public string? Description { get; set; }
}