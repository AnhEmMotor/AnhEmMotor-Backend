using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Seeders;

/// <summary>
/// Seeder để khởi tạo dữ liệu Permission vào database
/// </summary>
public static class PermissionDataSeeder
{
    /// <summary>
    /// Seed các permissions từ class Permissions vào database
    /// </summary>
    public static async Task SeedPermissionsAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var allPermissions = typeof(Domain.Constants.Permission.PermissionsList)
            .GetNestedTypes()
            .SelectMany(
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
            .Select(fieldInfo => fieldInfo.GetRawConstantValue() as string)
            .Where(permission => permission is not null)
            .ToList();

        var existingPermissions = await context.Permissions
            .Select(p => p.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var newPermissions = allPermissions
            .Except(existingPermissions)
            .Select(name => new Permission { Name = name! });

        if(newPermissions.Any())
        {
            await context.Permissions.AddRangeAsync(newPermissions, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
