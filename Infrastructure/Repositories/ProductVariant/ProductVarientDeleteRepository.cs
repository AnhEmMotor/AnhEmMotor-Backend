using Application.Interfaces.Repositories.ProductVariant;
using Infrastructure.DBContexts;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVarientDeleteRepository(ApplicationDBContext context) : IProductVarientDeleteRepository
    {
        public void Delete(ProductVariantEntity variant) { context.ProductVariants.Remove(variant); }
    }
}
