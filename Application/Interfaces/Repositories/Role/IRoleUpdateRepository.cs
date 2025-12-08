using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleUpdateRepository
    {
        Task AddPermissionsToRoleAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken = default);
        void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions);
    }
}
