using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantUpdateRepository
    {
        void Add(ProductVariantEntity variant);
    }
}
