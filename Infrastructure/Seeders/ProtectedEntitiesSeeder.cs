using Domain.Entities;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo SuperRoles và ProtectedUsers
/// </summary>
public static class ProtectedEntitiesSeeder
{
    /// <summary>
    /// Seed SuperRoles và ProtectedUsers từ appsettings
    /// </summary>
    public static async Task SeedProtectedEntitiesAsync(
        ApplicationDBContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        // Seed SuperRoles
        var superRoles = configuration.GetSection("ProtectedAuthorizationEntities:SuperRoles").Get<List<string>>() ?? [];
        foreach (var roleName in superRoles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }

        // Gán tất cả các permissions cho SuperRoles
        if (superRoles.Count != 0)
        {
            var allPermissions = await context.Permissions.ToListAsync();
            var superRoleEntities = await context.Roles
                .Where(r => r.Name != null && superRoles.Contains(r.Name))
                .ToListAsync();

            foreach (var role in superRoleEntities)
            {
                var existingPermissions = await context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();

                var permissionsToAdd = allPermissions
                    .Where(p => !existingPermissions.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id
                    });

                if (permissionsToAdd.Any())
                {
                    await context.RolePermissions.AddRangeAsync(permissionsToAdd);
                }
            }

            await context.SaveChangesAsync();
        }

        // Seed ProtectedUsers
        var protectedUserEmails = configuration.GetSection("ProtectedAuthorizationEntities:ProtectedUsers").Get<List<string>>() ?? [];
        foreach (var email in protectedUserEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null && superRoles.Count != 0)
            {
                // Tạo user mới với thông tin mặc định
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    Gender = Gender.Other,
                    PhoneNumber = "0000000000"
                };

                // Mật khẩu mặc định: Admin@123456 (nên thay đổi sau khi đăng nhập lần đầu)
                var result = await userManager.CreateAsync(newUser, "Admin@123456");
                if (result.Succeeded)
                {
                    // Gán SuperRole đầu tiên cho user
                    await userManager.AddToRoleAsync(newUser, superRoles[0]);
                }
            }
            else if (user is not null && superRoles.Count != 0)
            {
                // Đảm bảo user đã có SuperRole
                var userRoles = await userManager.GetRolesAsync(user);
                if (!userRoles.Any(r => superRoles.Contains(r)))
                {
                    await userManager.AddToRoleAsync(user, superRoles[0]);
                }
            }
        }
    }
}
