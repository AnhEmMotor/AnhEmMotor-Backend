using Domain.Constants.Permission;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class PermissionDataSeeder
{
    public static async Task SeedPermissionsAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var allPermissions = PermissionsList.GetMetadataList().Select(m => m.Id).ToList();
        var existingPermissions = await context.Permissions.ToListAsync(cancellationToken).ConfigureAwait(false);
        var newPermissions = allPermissions
            .Except(existingPermissions.Select(p => p.Name))
            .Select(name => new Permission { Name = name! })
            .ToList();
        if (newPermissions.Count != 0)
        {
            await context.Permissions.AddRangeAsync(newPermissions, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        var permissionsToDelete = existingPermissions
            .Where(p => !allPermissions.Contains(p.Name))
            .ToList();
        if (permissionsToDelete.Count != 0)
        {
            var permissionIds = permissionsToDelete.Select(p => p.Id).ToList();
            var rolePermissionsToDelete = await context.Set<RolePermission>()
                .Where(rp => permissionIds.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (rolePermissionsToDelete.Count != 0)
            {
                context.Set<RolePermission>().RemoveRange(rolePermissionsToDelete);
            }
            context.Permissions.RemoveRange(permissionsToDelete);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
