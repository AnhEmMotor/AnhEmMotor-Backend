using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Infrastructure.DBContexts;
using System;

namespace Infrastructure.Repositories.Role
{
    public class RoleUpdateRepository(ApplicationDBContext context) : IRoleUpdateRepository
    {
        public async Task AddPermissionsToRoleAsync(
            IEnumerable<RolePermission> rolePermissions,
            CancellationToken cancellationToken = default)
        { await context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken).ConfigureAwait(false); }

        public void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions)
        { context.RolePermissions.RemoveRange(rolePermissions); }
    }
}
