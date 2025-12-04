using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

/// <summary>
/// Attribute yêu cầu người dùng phải có ÍT NHẤT MỘT trong các quyền được chỉ định
/// </summary>
public class RequiresAnyPermissionsAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "RequiresAnyPermissions";

    public RequiresAnyPermissionsAttribute(params string[] permissions)
    {
        Policy = $"{PolicyPrefix}{string.Join(",", permissions)}";
    }
}
