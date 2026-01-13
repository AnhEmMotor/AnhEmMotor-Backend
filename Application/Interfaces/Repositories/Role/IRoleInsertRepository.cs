using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleInsertRepository
    {
        public Task<IdentityResult> CreateAsync(ApplicationRole role);
    }
}
