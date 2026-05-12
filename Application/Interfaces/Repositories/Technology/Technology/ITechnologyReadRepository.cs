using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using TechnologyEntity = Domain.Entities.Technology;

namespace Application.Interfaces.Repositories.Technology.Technology;

public interface ITechnologyReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<TechnologyEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<TechnologyEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<TechnologyEntity>> GetTechnologiesAsync(
        int? categoryId,
        int? brandId,
        CancellationToken cancellationToken = default);

    public Task<List<TechnologyEntity>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default);
}
