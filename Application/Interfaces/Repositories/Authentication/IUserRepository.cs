using Application.ApiContracts.User.Responses;
using Domain.Shared;
using Sieve.Models;

namespace Application.Interfaces.Repositories.Authentication;

public interface IUserRepository
{
    Task<PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken);
}