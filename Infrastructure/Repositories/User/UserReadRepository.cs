using Application.ApiContracts.Auth.Responses;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Responses;
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

        public async Task<ApplicationUser?> GetByRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken)
        {
            var result = await userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => string.Compare(u.RefreshToken, refreshToken) == 0, cancellationToken)
                .ConfigureAwait(false);
            return result;
        }

        public async Task<UserAuth> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var user = await userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => string.Compare(u.RefreshToken, refreshToken) == 0, cancellationToken)
                .ConfigureAwait(false);

            var roles = await userManager.GetRolesAsync(user!).ConfigureAwait(false);

            var UserAuth = new UserAuth
            {
                Id = user!.Id,
                UserName = user.UserName,
                Roles = [ .. roles ],
                Email = user.Email,
                FullName = user.FullName,
                Status = user.Status,
                AuthMethods = [ "pwd" ]
            };

            return UserAuth;
        }

        public async Task<UserAuth?> GetUserByIDAsync(Guid? idUser, CancellationToken cancellationToken)
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
            return new UserAuth()
            {
                Id = user.Id,
                UserName = user.UserName,
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
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false); return result; }

        public async Task<ApplicationUser?> FindUserByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.FindByEmailAsync(email).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false); return result;
        }

        public async Task<ApplicationUser?> FindUserByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        { 
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.FindByNameAsync(username).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false);
            return result;   
        }

        public async Task<bool> CheckPasswordAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.CheckPasswordAsync(user, password).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false); 
            return result;
        }

        public async Task<IList<string>> GetUserRolesAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
        { 
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.GetRolesAsync(user).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false); 
            return result;
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.GetUsersInRoleAsync(roleName).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false); 
            return result;
        }

        public async Task<IList<string>> GetRolesOfUserAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await userManager.GetRolesAsync(user).ContinueWith(t => t.Result, cancellationToken).ConfigureAwait(false); 
            return result;
        }
    }
}
