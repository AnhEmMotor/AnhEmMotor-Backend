using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVarientDeleteRepository
    {
        public void Delete(ProductVariantEntity variant);
    }
}
