using Application.Interfaces.Repositories.Role;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.Role
{
    public class RoleInsertRepository(RoleManager<ApplicationRole> roleManager) : IRoleInsertRepository
    {
        public async Task<IdentityResult> CreateAsync(ApplicationRole role)
        {
            return await roleManager.CreateAsync(role).ConfigureAwait(false);
        }
    }
}
