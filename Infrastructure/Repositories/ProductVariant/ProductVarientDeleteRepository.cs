using Application.Interfaces.Repositories.ProductVariant;
using Infrastructure.DBContexts;
using System.Linq;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVarientDeleteRepository(ApplicationDBContext context) : IProductVarientDeleteRepository
    {
        public void Delete(ProductVariantEntity variant)
        {
            if (variant.VariantOptionValues != null && variant.VariantOptionValues.Any())
            {
                context.VariantOptionValues.RemoveRange(variant.VariantOptionValues);
            }
            if (variant.ProductCollectionPhotos != null && variant.ProductCollectionPhotos.Any())
            {
                context.ProductCollectionPhotos.RemoveRange(variant.ProductCollectionPhotos);
            }
            context.ProductVariants.Remove(variant);
        }
    }
}
