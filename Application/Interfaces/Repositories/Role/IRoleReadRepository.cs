using Application.ApiContracts.Permission.Responses;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleReadRepository
    {
        public Task<List<ApplicationRole>> GetRolesByNameAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default);

        public Task<List<RolePermission>> GetRolesPermissionByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

        public Task<List<string>> GetPermissionsNameByRoleIdAsync(
            IEnumerable<Guid> roleIds,
            CancellationToken cancellationToken = default);

        public Task<List<string>> GetPermissionsNameByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default);

        public Task<List<RoleSelectResponse>> GetAllRolesSelectAsync(CancellationToken cancellationToken = default);

        public Task<bool> IsRoleExistAsync(
            string roleName,
            CancellationToken cancellationToken = default);

        public Task<ApplicationRole?> GetRoleByNameAsync(string roleName);

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName);
    }
}
