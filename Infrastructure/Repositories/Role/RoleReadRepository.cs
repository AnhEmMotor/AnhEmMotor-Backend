using Application.ApiContracts.Permission.Responses;
using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Role
{
    public class RoleReadRepository(ApplicationDBContext context, RoleManager<ApplicationRole> roleManager) : IRoleReadRepository
    {
        public Task<List<ApplicationRole>> GetRolesByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default)
        { return context.Roles.Where(r => names.Contains(r.Name!)).ToListAsync(cancellationToken); }

        public Task<List<RolePermission>> GetRolePermissionsByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public Task<List<string>> GetPermissionNamesByRoleIdsAsync(
            IEnumerable<Guid> roleIds,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public Task<List<string>> GetPermissionNamesByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission!.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<List<RoleSelectResponse>> GetAllRoleSelectsAsync(CancellationToken cancellationToken = default)
        {
            return context.Roles
                .Select(r => new RoleSelectResponse { ID = r.Id, Name = r.Name })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsRoleExistsAsync(
            string roleName,
            CancellationToken cancellationToken = default)
        {
            return await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false);
        }
    }
}
