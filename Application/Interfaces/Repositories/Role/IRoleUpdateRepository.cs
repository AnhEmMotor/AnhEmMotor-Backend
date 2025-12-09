using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleUpdateRepository
    {
        Task AddPermissionsToRoleAsync(
            IEnumerable<RolePermission> rolePermissions,
            CancellationToken cancellationToken = default);

        void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions);
    }
}
