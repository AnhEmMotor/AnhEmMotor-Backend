using Application.Common.Models;
using Domain.Primitives;
using Domain.Constants;
using Domain.Entities;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.Technology;

public interface ITechnologyCategoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<TechnologyCategory, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<List<TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}
