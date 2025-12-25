using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Domain.Primitives;
using Sieve.Models;
using System;

namespace Application.Interfaces.Repositories.User
{
    public interface IUserReadRepository
    {
        public Task<PagedResult<UserDTOForManagerResponse>> GetPagedListAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken);

        public Task<PagedResult<UserDTOForOutputResponse>> GetPagedListForOutputAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken);

        public Task<UserAuthDTO> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

        public Task<UserAuthDTO?> GetUserByIDAsync(Guid? idUser, CancellationToken cancellationToken);
    }
}
