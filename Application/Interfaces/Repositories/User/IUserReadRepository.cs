using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.User.Responses;
using Domain.Primitives;
using Sieve.Models;
using System;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserReadRepository
    {
        Task<PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken);

        Task<UserAuthDTO> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
