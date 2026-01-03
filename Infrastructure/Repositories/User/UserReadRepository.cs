using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories.User;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System;

namespace Infrastructure.Repositories.User
{
    public class UserReadRepository(UserManager<ApplicationUser> userManager, ISieveProcessor sieveProcessor) : IUserReadRepository
    {
        public async Task<PagedResult<UserDTOForManagerResponse>> GetPagedListAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsNoTracking();

            var countQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
            var totalCount = await countQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            var pagedQuery = sieveProcessor.Apply(sieveModel, query);
            var entities = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            var userResponses = new List<UserDTOForManagerResponse>();

            foreach(var user in entities)
            {
                var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

                var response = new UserDTOForManagerResponse
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Gender = user.Gender ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    Status = user.Status ?? "Active",
                    DeletedAt = user.DeletedAt,
                    Roles = [ .. roles ]
                };

                userResponses.Add(response);
            }

            return new PagedResult<UserDTOForManagerResponse>(
                userResponses,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }

        public async Task<PagedResult<UserDTOForOutputResponse>> GetPagedListForOutputAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsNoTracking();

            var countQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
            var totalCount = await countQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            var pagedQuery = sieveProcessor.Apply(sieveModel, query);
            var entities = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            var userResponses = new List<UserDTOForOutputResponse>();

            foreach(var user in entities)
            {
                var response = new UserDTOForOutputResponse { Id = user.Id, FullName = user.FullName ?? string.Empty, };

                userResponses.Add(response);
            }

            return new PagedResult<UserDTOForOutputResponse>(
                userResponses,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }

        public async Task<UserAuthDTO> GetUserByRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                    .FirstOrDefaultAsync(u => string.Compare(u.RefreshToken, refreshToken) == 0, cancellationToken)
                    .ConfigureAwait(false) ??
                throw new UnauthorizedException("Invalid refresh token.");
            if(user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedException("Refresh token has expired. Please login again.");
            }

            if(string.Compare(user.Status, UserStatus.Active) != 0 || user.DeletedAt != null)
            {
                throw new ForbiddenException("Account is not available.");
            }

            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

            return new UserAuthDTO()
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = [ .. roles ],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status,
                AuthMethods = [ "pwd" ]
            };
        }

        public async Task<UserAuthDTO?> GetUserByIDAsync(Guid? idUser, CancellationToken cancellationToken)
        {
            if(idUser == null)
                return null;
            var user = await userManager.Users
                .FirstOrDefaultAsync(
                    u => u.Id == idUser && string.Compare(u.Status, UserStatus.Active) == 0 && u.DeletedAt == null,
                    cancellationToken)
                .ConfigureAwait(false);
            if(user == null)
                return null;
            var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
            return new UserAuthDTO()
            {
                Id = user.Id,
                Username = user.UserName,
                Roles = [ .. roles ],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status,
            };
        }

        public async Task<ApplicationUser?> FindUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
        }

        public async Task<ApplicationUser?> FindUserByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        }

        public async Task<ApplicationUser?> FindUserByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            return await userManager.FindByNameAsync(username).ConfigureAwait(false);
        }

        public async Task<bool> CheckPasswordAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default)
        {
            return await userManager.CheckPasswordAsync(user, password).ConfigureAwait(false);
        }

        public async Task<IList<string>> GetUserRolesAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
        {
            return await userManager.GetRolesAsync(user).ConfigureAwait(false);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            CancellationToken cancellationToken = default)
        {
            return await userManager.GetUsersInRoleAsync(roleName).ConfigureAwait(false);
        }
    }
}
