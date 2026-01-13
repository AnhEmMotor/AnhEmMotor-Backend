using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleDeleteRepository
    {
        public Task<IdentityResult> DeleteAsync(ApplicationRole role);
    }
}
