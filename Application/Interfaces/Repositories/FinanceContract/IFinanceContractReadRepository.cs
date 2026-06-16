using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.FinanceContract;

public interface IFinanceContractReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
        where TResponse : class;

    public Task<Domain.Entities.FinanceContract?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
