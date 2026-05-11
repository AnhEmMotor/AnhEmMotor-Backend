using Application.Common.Models;
using Domain.Primitives;
using Domain.Constants;
using Sieve.Models;
using System.Linq.Expressions;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<ProductVariantEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default);

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
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            string? search = null);

        public Task<List<string>> GetUrlSlugsAsync(CancellationToken cancellationToken);
    }
}
