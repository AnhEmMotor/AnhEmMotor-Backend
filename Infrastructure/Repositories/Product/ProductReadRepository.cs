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
    { return context.GetQuery<ProductEntity>(mode).Include(p => p.ProductCategory).Include(p => p.Brand); }

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

        if(mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        } else if(mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(p => p.DeletedAt != null);
        }

        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
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

        if(mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(p => p.DeletedAt == null);
        } else if(mode == DataFetchMode.DeletedOnly)
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
            .Include(p => p.ProductVariants.Where(v => v.DeletedAt == null))
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
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

    public async Task<(List<ProductEntity> Items, int TotalCount)> GetPagedProductsAsync(
        string? search,
        List<string> statusIds,
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

        if(searchPattern != null)
        {
            query = query.Where(
                p => EF.Functions.Like(p.Name, searchPattern) ||
                    (p.ProductCategory != null && EF.Functions.Like(p.ProductCategory.Name, searchPattern)) ||
                    (p.Brand != null && EF.Functions.Like(p.Brand.Name, searchPattern)));
        }

        if(statusIds != null && statusIds.Count > 0)
        {
            query = query.Where(p => p.StatusId != null && statusIds.Contains(p.StatusId));
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

        return (entities, totalCount);
    }
}