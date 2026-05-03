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
        return context.GetQuery<ProductEntity>(mode).Include(p => p.ProductCategory).Include(p => p.Brand);
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
        IQueryable<ProductEntity> query = context.Products.IgnoreQueryFilters();
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
        IQueryable<ProductEntity> query = context.Products.IgnoreQueryFilters();
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

    public async Task<(List<ProductEntity> Items, int TotalCount, List<List<int>> GroupedOptionValueIds)> GetPagedProductsAsync(
        string? search,
        List<string> statusIds,
        List<int> categoryIds,
        List<int> optionValueIds,
        int page,
        int pageSize,
        string? filters,
        string? sorts,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var query = context.Products.IgnoreQueryFilters().Where(p => p.DeletedAt == null).AsNoTracking();
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
        var groupedByOption = new List<List<int>>();
        if (optionValueIds != null && optionValueIds.Count > 0)
        {
            var valueToOptionMapping = await context.OptionValues
                .Where(ov => optionValueIds.Contains(ov.Id))
                .Select(ov => new { ov.Id, ov.OptionId })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (valueToOptionMapping.Count == 0)
            {
                query = query.Where(p => false);
            } else
            {
                groupedByOption = [.. valueToOptionMapping
                    .GroupBy(x => x.OptionId)
                    .Select(g => g.Select(x => x.Id).ToList())];
                if (groupedByOption.Count == 1)
                {
                    var g1 = groupedByOption[0];
                    query = query.Where(
                        p => p.ProductVariants
                            .Any(
                                v => v.DeletedAt == null &&
                                        v.VariantOptionValues
                                            .Any(
                                                vov => vov.OptionValueId != null && g1.Contains(vov.OptionValueId.Value))));
                } else if (groupedByOption.Count == 2)
                {
                    var g1 = groupedByOption[0];
                    var g2 = groupedByOption[1];
                    query = query.Where(
                        p => p.ProductVariants
                            .Any(
                                v => v.DeletedAt == null &&
                                        v.VariantOptionValues
                                            .Any(
                                                vov => vov.OptionValueId != null && g1.Contains(vov.OptionValueId.Value)) &&
                                        v.VariantOptionValues
                                            .Any(
                                                vov => vov.OptionValueId != null && g2.Contains(vov.OptionValueId.Value))));
                } else if (groupedByOption.Count >= 3)
                {
                    var g1 = groupedByOption[0];
                    var g2 = groupedByOption[1];
                    var g3 = groupedByOption[2];
                    if (groupedByOption.Count == 3)
                    {
                        query = query.Where(
                            p => p.ProductVariants
                                .Any(
                                    v => v.DeletedAt == null &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g1.Contains(vov.OptionValueId.Value)) &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g2.Contains(vov.OptionValueId.Value)) &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g3.Contains(vov.OptionValueId.Value))));
                    } else
                    {
                        var g4 = groupedByOption[3];
                        query = query.Where(
                            p => p.ProductVariants
                                .Any(
                                    v => v.DeletedAt == null &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g1.Contains(vov.OptionValueId.Value)) &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g2.Contains(vov.OptionValueId.Value)) &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g3.Contains(vov.OptionValueId.Value)) &&
                                            v.VariantOptionValues
                                                .Any(
                                                    vov => vov.OptionValueId != null &&
                                                                    g4.Contains(vov.OptionValueId.Value))));
                    }
                }
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
            .ThenInclude(v => v.InputInfos.Where(ii => ii.DeletedAt == null && ii.InputReceipt!.DeletedAt == null))
            .ThenInclude(ii => ii.InputReceipt)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos.Where(oi => oi.DeletedAt == null && oi.OutputOrder!.DeletedAt == null))
            .ThenInclude(oi => oi.OutputOrder)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option);
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
        return (entities, totalCount, groupedByOption);
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
        IQueryable<Domain.Entities.ProductVariant> query = context.ProductVariants.IgnoreQueryFilters();
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
