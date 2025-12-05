using Domain.Constants;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantReadRepository
    {
        IQueryable<ProductVariantEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<ProductVariantEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<ProductVariantEntity?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.All);

        Task<IEnumerable<ProductVariantEntity>> GetByProductIdAsync(
            int productId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<IEnumerable<ProductVariantEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        Task<(List<ProductVariantEntity> Items, int TotalCount)> GetPagedVariantsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
