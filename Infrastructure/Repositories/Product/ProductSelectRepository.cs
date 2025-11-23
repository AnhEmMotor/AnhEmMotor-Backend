using Application.Interfaces.Repositories.Product;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BrandEntity = Domain.Entities.Brand;
using CategoryEntity = Domain.Entities.ProductCategory;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.Product;

public class ProductSelectRepository(ApplicationDBContext context) : IProductSelectRepository
{
    public Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<ProductEntity?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductCollectionPhotos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<ProductEntity?> GetProductWithDetailsByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = includeDeleted ? context.All<ProductEntity>() : context.Products.AsQueryable();

        return query
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
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<ProductVariantEntity?> GetVariantBySlugAsync(string slug, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = includeDeleted ? context.All<ProductVariantEntity>() : context.ProductVariants.AsQueryable();
        return query.FirstOrDefaultAsync(v => string.Compare(v.UrlSlug, slug) == 0, cancellationToken);
    }

    public IQueryable<ProductEntity> GetProducts()
    {
        return context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .AsNoTracking();
    }

    public IQueryable<ProductEntity> GetActiveProducts()
    {
        return context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants)
            .AsNoTracking();
    }

    public IQueryable<ProductVariantEntity> GetActiveVariants()
    {
        return context.ProductVariants.AsNoTracking();
    }

    public IQueryable<ProductEntity> GetDeletedProducts()
    {
        return context.DeletedOnly<ProductEntity>()
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .AsNoTracking();
    }

    public IQueryable<ProductVariantEntity> GetDeletedVariants()
    {
        return context.DeletedOnly<ProductVariantEntity>().AsNoTracking();
    }

    public IQueryable<ProductEntity> GetAllProducts()
    {
        return context.All<ProductEntity>()
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .AsNoTracking();
    }

    public Task<List<ProductEntity>> GetActiveProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProductEntity>> GetDeletedProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        var query = context.DeletedOnly<ProductEntity>()
            .Where(p => ids.Contains(p.Id));
        string deletedAtProp = AuditingProperties.DeletedAt;
        return query
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.VariantOptionValues)
                    .ThenInclude(vov => vov.OptionValue)
                        .ThenInclude(ov => ov!.Option)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.ProductCollectionPhotos)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.InputInfos)
            .Include(p => p.ProductVariants.Where(v => EF.Property<DateTimeOffset?>(v, deletedAtProp) == null))
                .ThenInclude(v => v.OutputInfos)
                    .ThenInclude(oi => oi.OutputOrder)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProductEntity>> GetAllProductsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.All<ProductEntity>()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProductVariantEntity>> GetVariantsByProductIdAsync(int productId, CancellationToken cancellationToken)
    {
        return context.ProductVariants
            .Where(v => v.ProductId == productId)
            .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
                    .ThenInclude(ov => ov!.Option)
            .ToListAsync(cancellationToken);
    }

    public Task<ProductVariantEntity?> GetVariantByIdWithOptionsAsync(int variantId, CancellationToken cancellationToken)
    {
        return context.ProductVariants
            .Include(v => v.VariantOptionValues)
                .ThenInclude(vov => vov.OptionValue)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
    }

    public Task<CategoryEntity?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken)
    {
        return context.ProductCategories
            .Where(c => c.Id == categoryId && EF.Property<DateTimeOffset?>(c, "DeletedAt") == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<BrandEntity?> GetBrandByIdAsync(int brandId, CancellationToken cancellationToken)
    {
        return context.Brands
            .Where(b => b.Id == brandId && EF.Property<DateTimeOffset?>(b, "DeletedAt") == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<OptionEntity?> GetOptionByIdAsync(int optionId, CancellationToken cancellationToken)
    {
        return context.Options.FirstOrDefaultAsync(o => o.Id == optionId, cancellationToken);
    }

    public Task<List<OptionEntity>> GetOptionsByIdsAsync(List<int> optionIds, CancellationToken cancellationToken)
    {
        return context.Options
            .Where(o => optionIds.Contains(o.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<OptionValueEntity>> GetOptionValuesByIdsAsync(List<int> optionValueIds, CancellationToken cancellationToken)
    {
        return context.OptionValues
            .Include(ov => ov.Option)
            .Where(ov => optionValueIds.Contains(ov.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<OptionValueEntity?> GetOptionValueByNameAsync(int optionId, string name, CancellationToken cancellationToken)
    {
        return context.OptionValues
            .FirstOrDefaultAsync(ov => ov.OptionId == optionId && ov.Name == name, cancellationToken);
    }

    public async Task<bool> OptionValuesBelongToOptionsAsync(
        List<int> optionValueIds,
        List<int> optionIds,
        CancellationToken cancellationToken)
    {
        var optionValues = await context.OptionValues
            .Where(ov => optionValueIds.Contains(ov.Id))
            .Select(ov => ov.OptionId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return optionValues.All(oid => oid.HasValue && optionIds.Contains(oid.Value));
    }
}