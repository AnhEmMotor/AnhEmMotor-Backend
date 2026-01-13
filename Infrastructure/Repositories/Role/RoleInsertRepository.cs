using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Infrastructure.Repositories.Role
{
    public class RoleInsertRepository(RoleManager<ApplicationRole> roleManager) : IRoleInsertRepository
    {
        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await roleManager.CreateAsync(role).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
