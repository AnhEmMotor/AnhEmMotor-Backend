using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantInsertRepository
    {
        void Add(ProductVariantEntity variant);
    }
}
