using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleUpdateRepository
    {
        public Task AddPermissionsToRoleAsync(
            IEnumerable<RolePermission> rolePermissions,
            CancellationToken cancellationToken = default);

        public void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions);
    }
}
