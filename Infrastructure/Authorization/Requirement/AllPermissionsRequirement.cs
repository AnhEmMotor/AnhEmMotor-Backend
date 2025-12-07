using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Requirement;

/// <summary>
/// Requirement yêu cầu TẤT CẢ các quyền
/// </summary>
public class AllPermissionsRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IEnumerable<string> Permissions { get; } = permissions;
}
