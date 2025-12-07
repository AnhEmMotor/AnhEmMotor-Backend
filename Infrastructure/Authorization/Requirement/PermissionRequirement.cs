using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization.Requirement;

/// <summary>
/// Requirement cho một quyền đơn lẻ
/// </summary>
public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
