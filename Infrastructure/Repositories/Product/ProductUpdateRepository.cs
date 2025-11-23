using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;
using VariantOptionValueEntity = Domain.Entities.VariantOptionValue;

namespace Infrastructure.Repositories.Product;

public class ProductUpdateRepository(ApplicationDBContext context) : IProductUpdateRepository
{
    public void Update(ProductEntity product)
    {
        context.Products.Update(product);
    }

    public void Restore(ProductEntity product)
    {
        context.RestoreSoftDeleteUsingSetColumn(product);
    }

    public void Restore(List<ProductEntity> products)
    {
        context.RestoreSoftDeleteUsingSetColumnRange(products);
    }

    public void RestoreVariant(ProductVariantEntity variant)
    {
        context.Restore(variant);
    }

    public void UpdateVariant(ProductVariantEntity variant)
    {
        context.ProductVariants.Update(variant);
    }

    public void DeleteVariant(ProductVariantEntity variant)
    {
        context.ProductVariants.Remove(variant);
    }

    public void DeleteVariantOptionValue(VariantOptionValueEntity variantOptionValue)
    {
        context.VariantOptionValues.Remove(variantOptionValue);
    }
}