using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.Product;

public interface IProductDeleteRepository
{
    void Delete(ProductEntity product);
    void Delete(List<ProductEntity> products);
    void DeleteVariant(ProductVariantEntity variant);
}
