using Application.Interfaces.Repositories.Authorization;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Authorization;

public class RolePermissionRepository(ApplicationDBContext context) : IRolePermissionRepository
{
    public async Task AddRangeAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken = default)
    {
        await context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
    }

    public void RemoveRange(IEnumerable<RolePermission> rolePermissions)
    {
        context.RolePermissions.RemoveRange(rolePermissions);
    }

    public async Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RolePermission>> GetByPermissionIdsAsync(IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => permissionIds.Contains(rp.PermissionId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetPermissionNamesByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission!.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetPermissionNamesByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .ToListAsync(cancellationToken);
    }
}
