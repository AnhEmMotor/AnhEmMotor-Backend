using Application.Common.Models;
using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleDeleteRepository
    {
        public Task<IdentityOperationResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken);
    }
}
