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
}
