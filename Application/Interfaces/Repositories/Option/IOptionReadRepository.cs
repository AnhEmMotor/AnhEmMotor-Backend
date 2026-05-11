using Application.Common.Models;
using Domain.Primitives;
using Domain.Constants;
using Sieve.Models;
using System.Linq.Expressions;
using OptionEntity = Domain.Entities.Option;

namespace Application.Interfaces.Repositories.Option;

public interface IOptionReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<OptionEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<List<OptionEntity>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<List<OptionEntity>> GetAllWithOptionsAsync(CancellationToken cancellationToken);
}
