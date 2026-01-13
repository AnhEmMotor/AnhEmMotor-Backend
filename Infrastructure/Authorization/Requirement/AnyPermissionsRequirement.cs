using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Requirement;

public class AnyPermissionsRequirement(IEnumerable<string> permissions) : IAuthorizationRequirement
{
    public IEnumerable<string> Permissions { get; } = permissions;
}
