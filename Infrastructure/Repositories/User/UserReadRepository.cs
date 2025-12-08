using Application.ApiContracts.User.Responses;
using Application.Interfaces.Repositories.User;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

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

            foreach (var user in entities)
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
                    Roles = [.. roles]
                };

                userResponses.Add(response);
            }

            return new Domain.Primitives.PagedResult<UserResponse>(
                userResponses,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }
    }
}
