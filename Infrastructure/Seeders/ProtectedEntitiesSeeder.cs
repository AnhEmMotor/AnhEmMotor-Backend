using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeders;

public static class ProtectedEntitiesSeeder
{
    public static async Task SeedProtectedEntitiesAsync(
        ApplicationDBContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        foreach(var roleName in superRoles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
            if(!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName }).ConfigureAwait(false);
            }
        }

        if(superRoles.Count != 0)
        {
            var allPermissions = await context.Permissions.ToListAsync(cancellationToken).ConfigureAwait(false);
            var superRoleEntities = await context.Roles
                .Where(r => r.Name != null && superRoles.Contains(r.Name))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach(var role in superRoleEntities)
            {
                var existingPermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var permissionsToAdd = allPermissions
                    .Where(p => !existingPermissions.Contains(p.Id))
                    .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id });

                if(permissionsToAdd.Any())
                {
                    await context.RolePermissions
                        .AddRangeAsync(permissionsToAdd, cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var protectedUsersSection = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers");
        var protectedUsers = new Dictionary<string, string>();

        var protectedUserList = protectedUsersSection.Get<List<string>>() ?? [];
        foreach(var entry in protectedUserList)
        {
            if(string.IsNullOrWhiteSpace(entry))
                continue;

            var parts = entry.Split(':');
            var email = parts[0]?.Trim() ?? string.Empty;
            var password = parts.Length > 1 ? parts[1]?.Trim() : null;

            if(!string.IsNullOrEmpty(email))
            {
                protectedUsers[email] = password ?? "DefaultProtectedUser@123456";
            }
        }

        var defaultRolesForNewUsers = configuration.GetSection("ProtectedAuthorizationEntities:DefaultRolesForNewUsers")
                .Get<List<string>>() ??
            [];

        foreach (var (email, password) in protectedUsers)
        {
            var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if(user is null)
            {
                var newUser = new ApplicationUser { UserName = email, Email = email, Status = UserStatus.Active, };

                var result = await userManager.CreateAsync(newUser, password).ConfigureAwait(false);
                if(result.Succeeded && superRoles.Count != 0)
                {
                    await userManager.AddToRoleAsync(newUser, superRoles[0]).ConfigureAwait(false);
                }
            }
        }

        if(defaultRolesForNewUsers.Count != 0)
        {
            foreach(var roleName in defaultRolesForNewUsers)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
                if(!roleExists)
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName }).ConfigureAwait(false);
                }
            }
        }
    }
}
