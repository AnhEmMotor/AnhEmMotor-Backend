using Application.ApiContracts.Permission.Responses;
using Application.Interfaces.Repositories.Role;
using Application.Sieve;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Infrastructure.Repositories.Role
{
    public class RoleReadRepository(
        ApplicationDBContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ISieveProcessor sieveProcessor) : IRoleReadRepository
    {
        public Task<List<ApplicationRole>> GetRolesByNameAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default)
        { return context.Roles.Where(r => names.Contains(r.Name!)).ToListAsync(cancellationToken); }

        public Task<List<ApplicationRole>> GetRolesByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        { return context.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(cancellationToken); }

        public Task<List<RolePermission>> GetRolesPermissionByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public Task<List<string>> GetPermissionsNameByRoleIdAsync(
            IEnumerable<Guid> roleIds,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission!.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public Task<List<string>> GetPermissionsNameByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            return context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission!.Name)
                .ToListAsync(cancellationToken);
        }

        public Task<List<RoleSelectResponse>> GetAllRolesSelectAsync(CancellationToken cancellationToken = default)
        {
            return context.Roles
                .Select(r => new RoleSelectResponse { ID = r.Id, Name = r.Name })
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResult<RoleSelectResponse>> GetPagedRolesSelectAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken = default)
        {
            var query = context.Roles.AsNoTracking()
                .Select(r => new RoleSelectResponse { ID = r.Id, Name = r.Name });

            SieveHelper.ApplyDefaultSorting(sieveModel);

            var countQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
            var totalCount = await countQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            var pagedQuery = sieveProcessor.Apply(sieveModel, query);
            var entities = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new PagedResult<RoleSelectResponse>(
                entities,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }

        public async Task<bool> IsRoleExistAsync(string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await roleManager.RoleExistsAsync(roleName)
                .ContinueWith(t => t.Result, cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await roleManager.FindByNameAsync(roleName)
                .ContinueWith(t => t.Result, cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.GetUsersInRoleAsync(roleName)
                .ContinueWith(t => t.Result, cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public Task<bool> HasAnyPermissionAsync(
            IEnumerable<Guid> roleIds,
            CancellationToken cancellationToken = default)
        { return context.RolePermissions.Where(rp => roleIds.Contains(rp.RoleId)).AnyAsync(cancellationToken); }
    }
}
