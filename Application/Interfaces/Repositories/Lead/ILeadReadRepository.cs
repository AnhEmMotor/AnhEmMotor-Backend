using Application.Common.Models;
using Domain.Primitives;
using Domain.Constants;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.Lead;

public interface ILeadReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Lead, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Lead?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Lead?> GetByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetAllLeadsWithActivitiesAsync(
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.Lead>> GetLoyaltyMembersAsync(
        string? search,
        CancellationToken cancellationToken = default);

    public Task<Domain.Entities.Lead?> GetByIdentificationNumberAsync(
        string identificationNumber,
        CancellationToken cancellationToken = default);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
