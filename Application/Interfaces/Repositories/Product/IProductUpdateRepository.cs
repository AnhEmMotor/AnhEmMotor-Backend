using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductUpdateRepository
{
    public void Update(ProductEntity product);

    public void Restore(ProductEntity product);

    public void Restore(IEnumerable<ProductEntity> products);
}
