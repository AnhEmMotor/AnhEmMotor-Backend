using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleInsertRepository
    {
        public Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken);
    }
}
