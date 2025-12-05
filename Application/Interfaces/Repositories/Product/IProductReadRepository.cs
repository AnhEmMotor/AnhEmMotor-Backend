using Domain.Constants;
using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductReadRepository
{
    IQueryable<ProductEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<ProductEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<ProductEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<ProductEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<IEnumerable<ProductEntity>> GetByIdWithVariantsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    Task<(List<ProductEntity> Items, int TotalCount)> GetPagedDeletedProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<(List<ProductEntity> Items, int TotalCount)> GetPagedProductsAsync(
        string? search,
        List<string> statusIds,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
