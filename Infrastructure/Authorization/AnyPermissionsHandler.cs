using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Infrastructure.Authorization;

/// <summary>
/// Handler xử lý kiểm tra ÍT NHẤT MỘT quyền
/// </summary>
public class AnyPermissionsHandler(
    ApplicationDBContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration) : AuthorizationHandler<AnyPermissionsRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionsRequirement requirement)
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

        // Lấy tất cả các quyền của người dùng
        var userPermissions = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync();

        // Kiểm tra xem người dùng có ÍT NHẤT MỘT quyền yêu cầu không
        if (requirement.Permissions.Any(p => userPermissions.Contains(p)))
        {
            context.Succeed(requirement);
        }
    }
}
