using Application.Interfaces.Repositories.ProductVariant;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Infrastructure.Repositories.ProductVariant
{
    public class ProductVariantInsertRepository : IProductVariantInsertRepository
    {
        public void Add(ProductVariantEntity variant) { 
            throw new NotImplementedException(); }
    }
}
