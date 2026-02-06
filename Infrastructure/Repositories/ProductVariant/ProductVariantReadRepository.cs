using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;

using Sieve.Services;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor) : IProductVariantReadRepository
    {
        public IQueryable<ProductVariantEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        { return context.GetQuery<ProductVariantEntity>(mode); }

        public Task<ProductVariantEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        public Task<ProductVariantEntity?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode)
                .FirstOrDefaultAsync(v => string.Compare(v.UrlSlug, slug) == 0, cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        public Task<IEnumerable<ProductVariantEntity>> GetByProductIdAsync(
            int productId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode)
                .Where(v => v.ProductId == productId && (mode != DataFetchMode.ActiveOnly || v.DeletedAt == null))
                .Include(v => v.Product)
                .Include(v => v.ProductCollectionPhotos)
                .Include(v => v.InputInfos)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .ThenInclude(ov => ov!.Option)
                .AsSplitQuery()
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<ProductVariantEntity>>(t => t.Result, cancellationToken);
        }

        public Task<IEnumerable<ProductVariantEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode)
                .Where(v => ids.Contains(v.Id))
                .Include(v => v.Product)
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<ProductVariantEntity>>(t => t.Result, cancellationToken);
        }

        public async Task<(List<ProductVariantEntity> Items, int TotalCount)> GetPagedVariantsAsync(
            int page,
            int pageSize,
            string? filters,
            string? sorts,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.GetQuery<ProductVariantEntity>(mode);

            if(mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            } else if(mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            }

            var normalizedPage = Math.Max(page, 1);
            var normalizedPageSize = Math.Max(pageSize, 1);

            var sieveModel = new SieveModel
            {
                Filters = filters,
                Sorts = sorts,
                Page = normalizedPage,
                PageSize = normalizedPageSize
            };

            query = sieveProcessor.Apply(sieveModel, query, applyPagination: false);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            IQueryable<ProductVariantEntity> dbQuery = query
                .Include(v => v.Product)
                .ThenInclude(p => p!.ProductCategory)
                .Include(v => v.Product)
                .ThenInclude(p => p!.Brand)
                .Include(v => v.Product)
                .ThenInclude(p => p!.ProductStatus)
                .Include(v => v.ProductCollectionPhotos)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .ThenInclude(ov => ov!.Option)

                .Include(v => v.InputInfos.Where(ii => ii.DeletedAt == null && ii.InputReceipt!.DeletedAt == null))
                .ThenInclude(ii => ii.InputReceipt)

                .Include(v => v.OutputInfos.Where(oi => oi.DeletedAt == null && oi.OutputOrder!.DeletedAt == null))
                .ThenInclude(oi => oi.OutputOrder);

            if(string.IsNullOrWhiteSpace(sorts))
            {
                dbQuery = dbQuery.OrderByDescending(v => v.Id);
            }

            var items = await dbQuery
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .AsSplitQuery()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return (items, totalCount);
        }
    }
}
