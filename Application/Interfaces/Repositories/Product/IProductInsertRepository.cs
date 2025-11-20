using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductInsertRepository
{
    Task AddAsync(ProductEntity product, CancellationToken cancellationToken);
}
