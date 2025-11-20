using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.Product;

public interface IProductUpdateRepository
{
    void Update(ProductEntity product);
    void Restore(ProductEntity product);
    void Restore(List<ProductEntity> products);
    void UpdateVariant(ProductVariantEntity variant);
    void DeleteVariant(ProductVariantEntity variant);
}
