using Domain.Constants;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantReadRepository
    {
        public IQueryable<ProductVariantEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<ProductVariantEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<ProductVariantEntity?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.All);

        public Task<IEnumerable<ProductVariantEntity>> GetByProductIdAsync(
            int productId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<IEnumerable<ProductVariantEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<(List<ProductVariantEntity> Items, int TotalCount)> GetPagedVariantsAsync(
            int page,
            int pageSize,
            string? filters,
            string? sorts,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}
