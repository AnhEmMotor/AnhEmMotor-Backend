using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantInsertRepository
    {
        public void Add(ProductVariantEntity variant);
    }
}
