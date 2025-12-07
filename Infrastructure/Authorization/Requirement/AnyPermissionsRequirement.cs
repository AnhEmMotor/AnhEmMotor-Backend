using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Requirement;

/// <summary>
/// Requirement yêu cầu ÍT NHẤT MỘT quyền
/// </summary>
public class AnyPermissionsRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IEnumerable<string> Permissions { get; } = permissions;
}
