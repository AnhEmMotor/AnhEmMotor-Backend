using Application.Common.Models;
using Domain.Entities;
using System;

namespace Application.Interfaces.Repositories.Role
{
    public interface IRoleInsertRepository
    {
        public Task<IdentityOperationResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken);
    }
}
