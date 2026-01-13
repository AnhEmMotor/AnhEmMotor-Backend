using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.Role
{
    public class RoleDeleteRepository(RoleManager<ApplicationRole> roleManager) : IRoleDeleteRepository
    {
        public async Task<IdentityResult> DeleteAsync(ApplicationRole role)
        {
            return await roleManager.DeleteAsync(role).ConfigureAwait(false);
        }
    }
}
