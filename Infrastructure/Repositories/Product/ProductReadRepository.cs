using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor) : IProductReadRepository
{
    public IQueryable<ProductEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ProductEntity>(mode)
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductTechnologies)
            .ThenInclude(pt => pt.Technology)
            .ThenInclude(t => t!.Category);
    }

    public Task<IEnumerable<ProductEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public Task<ProductEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        IQueryable<ProductEntity> query = context.Products;
        if (mode != DataFetchMode.ActiveOnly)
            query = query.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(p => p.DeletedAt != null);
        }
        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductTechnologies)
            .ThenInclude(pt => pt.Technology)
            .ThenInclude(t => t!.Category)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.InputInfos)
            .ThenInclude(ii => ii.InputReceipt)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<ProductEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public Task<ProductEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        IQueryable<ProductEntity> query = context.Products;
        if (mode != DataFetchMode.ActiveOnly)
            query = query.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(p => p.DeletedAt != null);
        }
        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductTechnologies)
            .ThenInclude(pt => pt.Technology)
            .ThenInclude(t => t!.Category)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.InputInfos)
            .ThenInclude(ii => ii.InputReceipt)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<IEnumerable<ProductEntity>> GetByIdWithVariantsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ProductEntity>(mode)
            .Where(p => ids.Contains(p.Id))
            .Include(p => p.ProductVariants)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<ProductEntity>>(t => t.Result, cancellationToken);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedDeletedProductsAsync(
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken)
    {
        var query = context.DeletedOnly<ProductEntity>();
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
        IQueryable<ProductEntity> dbQuery = query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.InputInfos)
            .ThenInclude(ii => ii.InputReceipt)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder);
        if (string.IsNullOrWhiteSpace(sorts))
        {
            dbQuery = dbQuery.OrderByDescending(p => p.DeletedAt);
        }
        var items = await dbQuery
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return (items, totalCount);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount, List<FilterGroup> GroupedOptionFilters)> GetPagedProductsAsync(
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
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var query = context.Products.AsNoTracking();
        if (searchPattern != null)
        {
            query = query.Where(
                p => EF.Functions.Like(p.Name, searchPattern) ||
                    (p.ProductCategory != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                    (p.Brand != null && EF.Functions.Like(p.Brand.Name, searchPattern)));
        }
        if (statusIds != null && statusIds.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && statusIds.Contains(p.StatusId));
        }
        if (categoryIds != null && categoryIds.Count > 0)
        {
            query = query.Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
        }
        if (brandIds != null && brandIds.Count > 0)
        {
            query = query.Where(p => p.BrandId != null && brandIds.Contains(p.BrandId.Value));
        }
        if (minPrice.HasValue || maxPrice.HasValue)
        {
            query = query.Where(
                p => p.ProductVariants
                    .Any(
                        v => v.DeletedAt == null &&
                                (!minPrice.HasValue || v.Price >= minPrice.Value) &&
                                (!maxPrice.HasValue || v.Price <= maxPrice.Value)));
        }
        var groupedOptionFilters = new List<FilterGroup>();
        if (optionValueIds != null && optionValueIds.Count > 0)
        {
            var valuesWithOption = await context.OptionValues
                .Where(ov => optionValueIds.Contains(ov.Id))
                .Select(ov => new { ov.Id, ov.OptionId, ov.Name })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (valuesWithOption.Count == 0)
            {
                query = query.Where(p => false);
            } else
            {
                var groups = valuesWithOption.GroupBy(v => v.OptionId)
                    .Select(
                        g => new
                        {
                            Ids = g.Select(x => x.Id).ToList(),
                            Names = g.Select(x => x.Name?.ToLower() ?? string.Empty).ToList()
                        })
                    .ToList();
                groupedOptionFilters = [.. groups.Select(g => new FilterGroup { Ids = g.Ids, Names = g.Names })];
                var variantSubquery = context.ProductVariants.Where(v => v.DeletedAt == null);
                if (minPrice.HasValue || maxPrice.HasValue)
                {
                    variantSubquery = variantSubquery.Where(
                        v => (!minPrice.HasValue || v.Price >= minPrice.Value) &&
                            (!maxPrice.HasValue || v.Price <= maxPrice.Value));
                }
                foreach (var group in groups)
                {
                    var ids = group.Ids;
                    var names = group.Names;
                    variantSubquery = variantSubquery.Where(
                        v => v.VariantOptionValues
                                .Any(vov => vov.OptionValueId != null && ids.Contains(vov.OptionValueId.Value)) ||
                            (v.ColorName != null &&
                                names.Any(n => v.ColorName.Contains(n))) ||
                            (v.VersionName != null &&
                                names.Any(n => v.VersionName.Contains(n))));
                }
                var matchingProductIds = variantSubquery.Select(v => v.ProductId);
                query = query.Where(p => matchingProductIds.Contains(p.Id));
            }
        }
        var sieveModel = new SieveModel
        {
            Filters = filters,
            Sorts = sorts,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
        query = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ProductEntity> dbQuery = query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.InputInfos)
            .ThenInclude(ii => ii.InputReceipt)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder);
        if (string.IsNullOrWhiteSpace(sorts))
        {
            dbQuery = dbQuery.OrderByDescending(p => p.CreatedAt);
        }
        var entities = await dbQuery
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return (entities, totalCount, groupedOptionFilters);
    }

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedProductsForPriceManagementAsync(
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);
        var query = context.Products.Where(p => p.DeletedAt == null).AsNoTracking();
        var effectiveSorts = string.IsNullOrWhiteSpace(sorts) ? "-CreatedAt" : sorts;
        var sieveModel = new SieveModel
        {
            Filters = filters,
            Sorts = effectiveSorts,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
        var filteredQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ProductEntity> dbQuery = sieveProcessor.Apply(sieveModel, query)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue);
        var entities = await dbQuery
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return (entities, totalCount);
    }

    public Task<Domain.Entities.ProductVariant?> GetByVariantSlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        IQueryable<Domain.Entities.ProductVariant> query = context.ProductVariants;
        if (mode != DataFetchMode.ActiveOnly)
            query = query.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(v => v.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(v => v.DeletedAt != null);
        }
        return query
            .Include(v => v.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Include(v => v.Product)
            .ThenInclude(p => p!.Brand)
            .Include(v => v.Product)
            .ThenInclude(p => p!.ProductTechnologies)
            .ThenInclude(pt => pt.Technology)
            .ThenInclude(t => t!.Category)
            .Include(v => v.Product)
            .ThenInclude(p => p!.ProductVariants.Where(pv => pv.DeletedAt == null))
            .ThenInclude(pv => pv.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .Include(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(v => v.ProductCollectionPhotos)
            .Include(v => v.InputInfos.Where(ii => ii.DeletedAt == null))
            .ThenInclude(ii => ii.InputReceipt)
            .Include(v => v.OutputInfos.Where(oi => oi.DeletedAt == null))
            .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => string.Compare(v.UrlSlug, slug) == 0, cancellationToken);
    }
}
