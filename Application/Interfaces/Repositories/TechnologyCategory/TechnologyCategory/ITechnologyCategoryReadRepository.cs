using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;

public interface ITechnologyCategoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.TechnologyCategory, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<List<Domain.Entities.TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default);
}

