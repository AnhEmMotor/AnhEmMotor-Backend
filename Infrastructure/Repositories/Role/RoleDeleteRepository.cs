using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Repositories.Role
{
    public class RoleDeleteRepository(RoleManager<ApplicationRole> roleManager) : IRoleDeleteRepository
    {
        public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await roleManager.DeleteAsync(role).ConfigureAwait(false);
            return result;
        }
    }
}
