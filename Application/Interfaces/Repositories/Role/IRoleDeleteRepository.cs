using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleDeleteRepository
    {
        public Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken);
    }
}
