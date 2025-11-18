using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.Product;

public class ProductUpdateRepository(ApplicationDBContext context) : IProductUpdateRepository
{
    public async Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateProductAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        context.Restore(product);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RestoreAsync(List<ProductEntity> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
        {
            context.Restore(product);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task AddVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken)
    {
        await context.ProductVariants.AddAsync(variant, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken)
    {
        context.ProductVariants.Update(variant);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken)
    {
        context.ProductVariants.Remove(variant);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveVariantAsync(int variantId, CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants.FindAsync([variantId], cancellationToken: cancellationToken).ConfigureAwait(false);

        if (variant != null)
        {
            context.ProductVariants.Remove(variant);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
