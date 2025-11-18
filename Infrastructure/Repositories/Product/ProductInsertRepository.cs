using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductInsertRepository(ApplicationDBContext context) : IProductInsertRepository
{
    public async Task<ProductEntity> AddAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        await context.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return product;
    }

    public async Task<ProductEntity> AddProductAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        await context.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
        return product;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
