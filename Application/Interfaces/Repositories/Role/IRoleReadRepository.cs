using Application.ApiContracts.Permission.Responses;
using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleReadRepository
    {
        public Task<List<ApplicationRole>> GetRolesByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default);

        public Task<List<RolePermission>> GetRolePermissionsByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

        public Task<List<string>> GetPermissionNamesByRoleIdsAsync(
            IEnumerable<Guid> roleIds,
            CancellationToken cancellationToken = default);

        public Task<List<string>> GetPermissionNamesByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

        public Task<List<RoleSelectResponse>> GetAllRoleSelectsAsync(CancellationToken cancellationToken = default);

        public Task<bool> IsRoleExistsAsync(
            string roleName,
            CancellationToken cancellationToken = default);
    }
}
