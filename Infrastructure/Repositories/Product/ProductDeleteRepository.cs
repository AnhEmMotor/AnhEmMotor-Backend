using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductDeleteRepository(ApplicationDBContext context) : IProductDeleteRepository
{
    public void Delete(ProductEntity product)
    {
        context.Products.Remove(product);
    }

    public void Delete(List<ProductEntity> products)
    {
        context.Products.RemoveRange(products);
    }

    public async Task DeleteProductAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteProductsAsync(List<ProductEntity> products, CancellationToken cancellationToken)
    {
        context.Products.RemoveRange(products);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
