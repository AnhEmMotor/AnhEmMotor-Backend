using Domain.Constants;
using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputReadRepository
{
    public IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<OutputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<OutputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<OutputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<OutputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<long> GetStockQuantityByVariantIdAsync(int variantId, CancellationToken cancellationToken);
}
