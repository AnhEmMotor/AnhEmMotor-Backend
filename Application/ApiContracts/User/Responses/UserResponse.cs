using Application.ApiContracts.Permission.Responses;

namespace Application.ApiContracts.User.Responses;

public class UserResponse
{
    public Guid? Id { get; set; }

    public string? UserName { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string? FullName { get; set; } = string.Empty;

    public string? Gender { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;

    public string? Status { get; set; } = string.Empty;

    public List<PermissionResponse>? Permissions { get; set; }
}
