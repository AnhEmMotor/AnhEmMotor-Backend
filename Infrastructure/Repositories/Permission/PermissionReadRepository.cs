using Application.Interfaces.Repositories.Permission;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using PermissionEntity = Domain.Entities.Permission;

namespace Infrastructure.Repositories.Permission
{
    public class PermissionReadRepository(ApplicationDBContext context) : IPermissionReadRepository
    {
        public Task<List<PermissionEntity>> GetPermissionsByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default)
        {
            return context.Permissions.Where(p => names.Contains(p.Name)).ToListAsync(cancellationToken);
        }

        public Task<List<RolePermission>> GetRolePermissionsByPermissionIdsAsync(
            IEnumerable<int> permissionIds,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => permissionIds.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CheckUserPermissionsAsync(
            Guid userId,
            IEnumerable<string> permissionNames,
            CancellationToken cancellationToken = default)
        {
            var roleIds = await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var hasPermission = await context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Where(rp => rp.Permission != null && permissionNames.Contains(rp.Permission.Name))
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);
            return hasPermission;
        }

        public async Task<bool> HasAnyPermissionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var roleIds = await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            return await context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public Task<List<ApplicationUser>> GetUsersWithPermissionAsync(
            string permissionName,
            CancellationToken cancellationToken = default)
        {
            var roleIdsWithPermission = context.RolePermissions
                .Where(rp => rp.Permission != null && rp.Permission.Name == permissionName)
                .Select(rp => rp.RoleId);
            var userIdsWithPermission = context.UserRoles
                .Where(ur => roleIdsWithPermission.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct();
            return context.Users
                .Where(u => userIdsWithPermission.Contains(u.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
