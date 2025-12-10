using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantUpdateRepository
    {
        public void Restore(ProductVariantEntity variant);

        public void Update(ProductVariantEntity variant);
    }
}
