using Application.ApiContracts.Auth.Requests;
using Application.ApiContracts.User.Responses;
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
        public async Task<PagedResult<UserResponse>> GetPagedListAsync(
            SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsNoTracking();

            var countQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
            var totalCount = await countQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            var pagedQuery = sieveProcessor.Apply(sieveModel, query);
            var entities = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            var userResponses = new List<UserResponse>();

            foreach(var user in entities)
            {
                var roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);

                var response = new UserResponse
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

            return new PagedResult<UserResponse>(
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
            if (idUser == null) return null;
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == idUser
                                       && u.Status == UserStatus.Active 
                                       && u.DeletedAt == null,          
                cancellationToken)
                .ConfigureAwait(false);
            if (user == null) return null;
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
    }
}
