using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductInsertRepository(ApplicationDBContext context) : IProductInsertRepository
{
    public async Task AddAsync(ProductEntity product, CancellationToken cancellationToken)
    {
        await context.Products.AddAsync(product, cancellationToken).ConfigureAwait(false);
    }
}
