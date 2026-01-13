using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Attribute;

public class RequiresAnyPermissionsAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "RequiresAnyPermissions";

    public RequiresAnyPermissionsAttribute(params string[] permissions)
    { Policy = $"{PolicyPrefix}{string.Join(",", permissions)}"; }
}
