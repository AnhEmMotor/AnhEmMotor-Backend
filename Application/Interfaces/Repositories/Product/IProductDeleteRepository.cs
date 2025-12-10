using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductDeleteRepository
{
    public void Delete(ProductEntity product);

    public void Delete(IEnumerable<ProductEntity> products);
}
