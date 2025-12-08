using Application.Interfaces.Repositories.Permission;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using PermissionEntity = Domain.Entities.Permission;

namespace Infrastructure.Repositories.Permission
{
    public class PermissionReadRepository(ApplicationDBContext context) : IPermissionReadRepository
    {
        public Task<List<PermissionEntity>> GetPermissionsByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
        { return context.Permissions.Where(p => names.Contains(p.Name)).ToListAsync(cancellationToken); }

        public Task<List<RolePermission>> GetRolePermissionsByPermissionIdsAsync(
        IEnumerable<int> permissionIds,
        CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => permissionIds.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);
        }
    }
}