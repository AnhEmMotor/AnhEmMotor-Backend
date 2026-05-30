using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;
using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<ProductEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    public Task<IEnumerable<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<ProductEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<ProductEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<ProductEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<ProductEntity>> GetByIdWithVariantsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<(List<ProductEntity> Items, int TotalCount)> GetPagedDeletedProductsAsync(
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken);

    public Task<(List<ProductEntity> Items, int TotalCount, List<FilterGroup> GroupedOptionFilters)> GetPagedProductsAsync(
        string? search,
        List<string> statusIds,
        List<int> categoryIds,
        List<int> brandIds,
        List<int> optionValueIds,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken);

    public Task<(List<ProductEntity> Items, int TotalCount)> GetPagedProductsForPriceManagementAsync(
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken);

    public Task<Domain.Entities.ProductVariant?> GetByVariantSlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    public Task<List<ProductEntity>> GetAllProductsWithInventoryDetailsAsync(CancellationToken cancellationToken);

    public Task<Domain.Entities.ProductVariant?> GetVariantByIdWithDetailsAsync(int variantId, CancellationToken cancellationToken);
}
