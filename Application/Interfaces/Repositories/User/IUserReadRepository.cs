using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.User.Responses;
using Domain.Primitives;
using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserReadRepository
    {
        Task<PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken);
        Task<UserAuthDTO> GetUserByRefreshTokenAsync(string refreshToken);
    }
}
