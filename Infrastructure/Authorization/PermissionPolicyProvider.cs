using Infrastructure.Authorization.Requirement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization;

/// <summary>
/// Policy provider tự động tạo policies dựa trên tên
/// </summary>
public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private const string HasPermissionPrefix = "HasPermission";
    private const string RequiresAllPermissionsPrefix = "RequiresAllPermissions";
    private const string RequiresAnyPermissionsPrefix = "RequiresAnyPermissions";

    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if(policyName.StartsWith(HasPermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[HasPermissionPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        if(policyName.StartsWith(RequiresAllPermissionsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionsString = policyName[RequiresAllPermissionsPrefix.Length..];
            var permissions = permissionsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new AllPermissionsRequirement(permissions));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        if(policyName.StartsWith(RequiresAnyPermissionsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionsString = policyName[RequiresAnyPermissionsPrefix.Length..];
            var permissions = permissionsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new AnyPermissionsRequirement(permissions));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
