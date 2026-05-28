using Application.Interfaces.Repositories.Permission;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    }
}
