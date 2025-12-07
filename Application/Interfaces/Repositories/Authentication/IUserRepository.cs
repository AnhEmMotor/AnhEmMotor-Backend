using Application.ApiContracts.User.Responses;
using Sieve.Models;

namespace Application.Interfaces.Repositories.Authentication;

public interface IUserRepository
{
    Task<Domain.Primitives.PagedResult<UserResponse>> GetPagedListAsync(SieveModel sieveModel, CancellationToken cancellationToken);
}