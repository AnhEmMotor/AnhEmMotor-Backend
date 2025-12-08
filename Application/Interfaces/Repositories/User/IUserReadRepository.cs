using Application.ApiContracts.User.Responses;
using Sieve.Models;
using System;
using Domain.Primitives;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserReadRepository
    {
        Task<PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken);
    }
}
