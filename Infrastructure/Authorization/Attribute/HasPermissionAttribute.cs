using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Attribute;

public class HasPermissionAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "HasPermission";

    public HasPermissionAttribute(string permission) { Policy = $"{PolicyPrefix}{permission}"; }
}
