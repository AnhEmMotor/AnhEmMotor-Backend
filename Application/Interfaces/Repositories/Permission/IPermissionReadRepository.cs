using Domain.Entities;
using System;
using PermissionEntity = Domain.Entities.Permission;

namespace Application.Interfaces.Repositories.Permission
{
    public interface IPermissionReadRepository
    {
        public Task<List<PermissionEntity>> GetPermissionsByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default);

        public Task<List<RolePermission>> GetRolePermissionsByPermissionIdsAsync(
            IEnumerable<int> permissionIds,
            CancellationToken cancellationToken = default);

        public Task<bool> CheckUserPermissionsAsync(
            Guid userId,
            IEnumerable<string> permissionNames,
            CancellationToken cancellationToken = default);
    }
}

