using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.VariantOptionValue;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductUpdateRepository(ApplicationDBContext context) : IProductUpdateRepository
{
    public void Update(ProductEntity product)
    {
        context.Products.Update(product);
    }

    public void Restore(ProductEntity product)
    {
        context.RestoreDeleteUsingSetColumn(product);
    }

    public void Restore(IEnumerable<ProductEntity> products)
    {
        context.RestoreDeleteUsingSetColumnRange(products);
    }
}