using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductReadRepository(ApplicationDBContext context) : IProductReadRepository
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
        DataFetchMode mode = DataFetchMode.All)
    {
        return GetQueryable(mode)
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
        return context.GetQuery<ProductEntity>(mode)
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder)
            .IgnoreQueryFilters()
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
        CancellationToken cancellationToken)
    {
        var query = context.DeletedOnly<ProductEntity>();

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
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
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.DeletedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Max(pageSize, 1);
        var searchPattern = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        var query = context.GetQuery<ProductEntity>(DataFetchMode.ActiveOnly).AsNoTracking();

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

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var entities = await query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.OutputInfos)
            .ThenInclude(oi => oi.OutputOrder)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants)
            .ThenInclude(v => v.VariantOptionValues)
            .ThenInclude(vov => vov.OptionValue)
            .ThenInclude(ov => ov!.Option)
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return (entities, totalCount);
    }
}