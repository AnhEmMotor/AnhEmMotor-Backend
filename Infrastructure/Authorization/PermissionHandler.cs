using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Infrastructure.Authorization;

public class PermissionHandler(
    ApplicationDBContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration,
    IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var cancellationToken = httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId is null)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if(user is null)
        {
            return;
        }

        var userRoles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        if(userRoles.Any(role => superRoles.Contains(role)))
        {
            context.Succeed(requirement);
            return;
        }

        var roleIds = roleManager.Roles
            .Where(r => r.Name != null && userRoles.Contains(r.Name))
            .Select(r => r.Id)
            .ToList();

        var hasPermission = await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Where(rp => rp.Permission != null && rp.Permission.Name == requirement.Permission)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if(hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}