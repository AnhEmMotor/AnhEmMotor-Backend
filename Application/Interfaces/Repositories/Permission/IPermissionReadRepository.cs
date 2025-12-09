using Domain.Entities;
using System;
using PermissionEntity = Domain.Entities.Permission;

namespace Application.Interfaces.Repositories.Permission
{
    public interface IPermissionReadRepository
    {
        Task<List<PermissionEntity>> GetPermissionsByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default);

        Task<List<RolePermission>> GetRolePermissionsByPermissionIdsAsync(
            IEnumerable<int> permissionIds,
            CancellationToken cancellationToken = default);
    }
}
