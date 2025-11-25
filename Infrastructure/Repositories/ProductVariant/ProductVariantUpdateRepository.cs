using Application.Interfaces.Repositories.ProductVariant;
using Infrastructure.DBContexts;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantUpdateRepository(ApplicationDBContext context) : IProductVariantUpdateRepository
    {
        public void Restore(ProductVariantEntity variant)
        {
            context.Restore(variant);
        }

        public void Update(ProductVariantEntity variant)
        {
            context.ProductVariants.Update(variant);
        }
    }
}
