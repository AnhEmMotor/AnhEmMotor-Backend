using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantUpdateRepository
    {
        void Restore(ProductVariantEntity variant);
        void Update(ProductVariantEntity variant);
    }
}
