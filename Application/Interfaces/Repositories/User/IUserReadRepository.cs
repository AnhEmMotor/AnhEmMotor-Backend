using Application.ApiContracts.Auth.Responses;
using Application.ApiContracts.UserManager.Responses;
using Domain.Entities;
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

        public Task<UserAuth> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

        public Task<ApplicationUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);

        public Task<UserAuth?> GetUserByIDAsync(Guid? idUser, CancellationToken cancellationToken);

        public Task<ApplicationUser?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        public Task<ApplicationUser?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        public Task<ApplicationUser?> FindUserByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default);

        public Task<bool> CheckPasswordAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default);

        public Task<IList<string>> GetUserRolesAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            CancellationToken cancellationToken = default);

        public Task<IList<string>> GetRolesOfUserAsync(ApplicationUser user, CancellationToken cancellationToken);
    }
}
