using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantReadRepository(
        ApplicationDBContext context,
        ISieveProcessor sieveProcessor,
        ISievePaginator paginator) : IProductVariantReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<ProductVariantEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<ProductVariantEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        internal IQueryable<ProductVariantEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode);
        }

        public async Task<ProductVariantEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return await context.ProductVariants
                .Include(v => v.ProductVariantColors)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .ThenInclude(ov => ov!.Option)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<ProductVariantEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.ProductVariants.AsQueryable();
            // Note: ProductVariant doesn't have IsDeleted property.
            return await query
                .Include(v => v.ProductVariantColors)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public Task<ProductVariantEntity?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return context.GetQuery<ProductVariantEntity>(mode)
                .Include(v => v.ProductVariantColors)
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
                .Include(v => v.InventoryReceiptInfos)
                .Include(v => v.ProductVariantColors)
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
                .ThenInclude(p => p!.ProductCategory)
                .Include(v => v.ProductCollectionPhotos)
                .Include(v => v.ProductVariantColors)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .AsSplitQuery()
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<ProductVariantEntity>>(t => t.Result, cancellationToken);
        }

        public async Task<(List<ProductVariantEntity> Items, int TotalCount)> GetPagedVariantsAsync(
            int page,
            int pageSize,
            string? filters,
            string? sorts,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            string? search = null)
        {
            var query = context.GetQuery<ProductVariantEntity>(mode);
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            } else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(v => v.Product != null && v.Product.DeletedAt == null);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchPattern = $"%{search.Trim()}%";
                query = query.Where(
                    v => v.Product != null &&
                        (EF.Functions.Like(v.Product.Name!, searchPattern) ||
                            (v.VariantName != null && EF.Functions.Like(v.VariantName, searchPattern)) ||
                            v.VariantOptionValues
                                .Any(
                                    vov => vov.OptionValue != null &&
                                                    EF.Functions.Like(vov.OptionValue.Name!, searchPattern))));
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
                .Include(v => v.ProductVariantColors)
                .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                .ThenInclude(ov => ov!.Option)
                .Include(
                    v => v.InventoryReceiptInfos
                        .Where(ii => ii.DeletedAt == null && ii.InventoryReceipt!.DeletedAt == null))
                .ThenInclude(ii => ii.InventoryReceipt)
                .Include(v => v.OutputInfos.Where(oi => oi.DeletedAt == null && oi.OutputOrder!.DeletedAt == null))
                .ThenInclude(oi => oi.OutputOrder);
            if (string.IsNullOrWhiteSpace(sorts))
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

        public Task<List<string>> GetUrlSlugsAsync(CancellationToken cancellationToken)
        {
            return context.GetQuery<ProductVariantEntity>(DataFetchMode.ActiveOnly)
                .Where(v => !string.IsNullOrEmpty(v.UrlSlug))
                .Select(v => v.UrlSlug!)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
