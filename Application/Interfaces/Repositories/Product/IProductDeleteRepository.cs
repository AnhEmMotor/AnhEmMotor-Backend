using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductDeleteRepository
{
    void Delete(ProductEntity product);
    void Delete(List<ProductEntity> products);
}
