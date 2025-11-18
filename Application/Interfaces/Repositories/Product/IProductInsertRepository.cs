using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductInsertRepository
{
    Task<ProductEntity> AddAsync(ProductEntity product, CancellationToken cancellationToken);
    Task<ProductEntity> AddProductAsync(ProductEntity product, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
