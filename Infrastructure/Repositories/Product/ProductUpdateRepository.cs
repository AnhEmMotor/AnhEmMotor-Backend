using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.Product;

public class ProductUpdateRepository(ApplicationDBContext context) : IProductUpdateRepository
{
    public void Update(ProductEntity product)
    {
        context.Products.Update(product);
    }

    public void Restore(ProductEntity product)
    {
        context.Restore(product);
    }

    public void Restore(List<ProductEntity> products)
    {
        foreach (var product in products)
        {
            context.Restore(product);
        }
    }

    public void UpdateVariant(ProductVariantEntity variant)
    {
        context.ProductVariants.Update(variant);
    }

    public void DeleteVariant(ProductVariantEntity variant)
    {
        context.ProductVariants.Remove(variant);
    }
}