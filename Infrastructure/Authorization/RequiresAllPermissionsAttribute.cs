using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

/// <summary>
/// Attribute yêu cầu người dùng phải có TẤT CẢ các quyền được chỉ định
/// </summary>
public class RequiresAllPermissionsAttribute : AuthorizeAttribute
{
    private const string PolicyPrefix = "RequiresAllPermissions";

    public RequiresAllPermissionsAttribute(params string[] permissions)
    {
        Policy = $"{PolicyPrefix}{string.Join(",", permissions)}";
    }
}
