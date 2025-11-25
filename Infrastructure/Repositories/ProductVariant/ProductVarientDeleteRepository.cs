using ProductVariantEntity = Domain.Entities.ProductVariant;
using Application.Interfaces.Repositories.ProductVariant;
using Application;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVarientDeleteRepository(ApplicationDBContext context) : IProductVarientDeleteRepository
    {
        public void Delete(ProductVariantEntity variant)
        {
            context.ProductVariants.Remove(variant);
        }
    }
}
