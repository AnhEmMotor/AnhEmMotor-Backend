using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVariantDeleteRepository
    {
        public void Delete(ProductVariantEntity variant);
    }
}
