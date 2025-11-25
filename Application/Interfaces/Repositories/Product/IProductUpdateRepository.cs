using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductUpdateRepository
{
    void Update(ProductEntity product);

    void Restore(ProductEntity product);

    void Restore(IEnumerable<ProductEntity> products);
}
