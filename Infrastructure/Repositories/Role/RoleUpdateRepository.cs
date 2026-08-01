using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Repositories.Role
{
    public class RoleUpdateRepository(ApplicationDBContext context, RoleManager<ApplicationRole> roleManager) : IRoleUpdateRepository
    {
        public async Task AddPermissionsToRoleAsync(
            IEnumerable<RolePermission> rolePermissions,
            CancellationToken cancellationToken = default)
        {
            await context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken).ConfigureAwait(false);
        }

        public void RemovePermissionsFromRole(IEnumerable<RolePermission> rolePermissions)
        {
            context.RolePermissions.RemoveRange(rolePermissions);
        }

        public async Task RenameRoleAsync(ApplicationRole role, string newName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await roleManager.SetRoleNameAsync(role, newName).ConfigureAwait(false);
        }
    }
}
