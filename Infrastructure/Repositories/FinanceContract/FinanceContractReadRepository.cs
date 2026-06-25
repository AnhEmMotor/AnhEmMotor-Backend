using Application.Features.FinanceContracts.Queries.GetFinanceContractsList;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;

namespace Infrastructure.Repositories.FinanceContract;

public class FinanceContractReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IFinanceContractReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var query = context.FinanceContracts.AsQueryable();
        var filterResult = FinanceContractKeywordFilter.Apply(query, sieveModel);
        return paginator.ApplyAsync<Domain.Entities.FinanceContract, TResponse>(
            filterResult.Query,
            filterResult.SieveModel,
            mode,
            cancellationToken);
    }

    public Task<Domain.Entities.FinanceContract?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.FinanceContracts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
