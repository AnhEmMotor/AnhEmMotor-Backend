using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

/// <summary>
/// Attribute yêu cầu người dùng phải có ít nhất một quyền cụ thể
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "HasPermission";

    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PolicyPrefix}{permission}";
    }
}
