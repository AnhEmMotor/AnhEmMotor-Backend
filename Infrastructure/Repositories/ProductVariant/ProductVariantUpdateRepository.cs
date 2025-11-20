using Application.Interfaces.Repositories.ProductVariant;
using Infrastructure.DBContexts;
using ProductVariantEntites = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantUpdateRepository(ApplicationDBContext context) : IProductVariantUpdateRepository
    {
        public void Add(ProductVariantEntites variant)
        {
            context.ProductVariants.Add(variant);
        }
    }
}
