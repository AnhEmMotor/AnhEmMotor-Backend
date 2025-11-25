using ProductVariantEntity = Domain.Entities.ProductVariant;
using Application.Interfaces.Repositories.ProductVariant;
using Application;
using Application.Interfaces;
using Application.Interfaces.Repositories;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantInsertRepository : IProductVariantInsertRepository
    {
        public void Add(ProductVariantEntity variant)
        {
            throw new NotImplementedException();
        }
    }
}
