using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.Product;

public class ProductDeleteRepository(ApplicationDBContext context) : IProductDeleteRepository
{
    public void Delete(ProductEntity product)
    {
        context.SoftDeleteUsingSetColumn(product);
    }

    public void Delete(List<ProductEntity> products)
    {
        context.SoftDeleteUsingSetColumnRange(products);
    }

    public void DeleteVariant(ProductVariantEntity variant)
    {
        context.ProductVariants.Remove(variant);
    }
}
