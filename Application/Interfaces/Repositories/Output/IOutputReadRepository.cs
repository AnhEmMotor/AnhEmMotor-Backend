using Domain.Enums;
using OutputEntity = Domain.Entities.Output;

namespace Application.Interfaces.Repositories.Output;

public interface IOutputReadRepository
{
    IQueryable<OutputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<OutputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<OutputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<OutputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<OutputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<long> GetStockQuantityByVariantIdAsync(int variantId, CancellationToken cancellationToken);
}
