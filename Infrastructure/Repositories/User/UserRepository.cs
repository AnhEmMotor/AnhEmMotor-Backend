using Domain.Entities;
using Domain.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;
using Sieve.Models;
using Application.ApiContracts.User.Responses;
using Application.Interfaces.Repositories.Authentication;

namespace Infrastructure.Repositories;

public class UserRepository(
    UserManager<ApplicationUser> userManager,
    ISieveProcessor sieveProcessor)
    : IUserRepository
{
    public async Task<PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken)
    {
        var query = userManager.Users.AsNoTracking();

        var countQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        var totalCount = await countQuery.CountAsync(cancellationToken);

        var pagedQuery = sieveProcessor.Apply(sieveModel, query);
        var entities = await pagedQuery.ToListAsync(cancellationToken);

        var userResponses = new List<UserResponse>();

        foreach (var user in entities)
        {
            var roles = await userManager.GetRolesAsync(user);

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

        return new PagedResult<UserResponse>(
            userResponses,
            totalCount,
            sieveModel.Page ?? 1,
            sieveModel.PageSize ?? 10);
    }
}