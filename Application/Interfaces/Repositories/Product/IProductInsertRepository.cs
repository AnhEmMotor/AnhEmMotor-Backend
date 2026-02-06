using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductInsertRepository
{
    public void Add(ProductEntity product);
}
