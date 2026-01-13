using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Attribute;

public class RequiresAllPermissionsAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "RequiresAllPermissions";

    public RequiresAllPermissionsAttribute(params string[] permissions)
    { Policy = $"{PolicyPrefix}{string.Join(",", permissions)}"; }
}
