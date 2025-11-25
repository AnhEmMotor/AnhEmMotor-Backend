using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.ProductVariant
{
    public interface IProductVarientDeleteRepository
    {
        void Delete(ProductVariantEntity variant);
    }
}
