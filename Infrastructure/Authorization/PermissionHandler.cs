using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Infrastructure.Authorization;

/// <summary>
/// Handler xử lý kiểm tra quyền đơn lẻ
/// </summary>
public class PermissionHandler(
    ApplicationDBContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        // Lấy danh sách vai trò của người dùng
        var userRoles = await userManager.GetRolesAsync(user);

        // Kiểm tra SuperRoles từ appsettings
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        if (userRoles.Any(role => superRoles.Contains(role)))
        {
            context.Succeed(requirement);
            return;
        }

        // Lấy ID của các vai trò
        var roleIds = roleManager.Roles
            .Where(r => r.Name != null && userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToList();

        // Kiểm tra quyền từ database
        var hasPermission = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Where(rp => rp.Permission != null && rp.Permission.Name == requirement.Permission)
            .AnyAsync();

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
