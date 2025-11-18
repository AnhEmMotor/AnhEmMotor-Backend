using ProductEntity = Domain.Entities.Product;

namespace Application.Interfaces.Repositories.Product;

public interface IProductDeleteRepository
{
    void Delete(ProductEntity product);
    void Delete(List<ProductEntity> products);
    Task DeleteProductAsync(ProductEntity product, CancellationToken cancellationToken);
    Task DeleteProductsAsync(List<ProductEntity> products, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
