using ProductEntity = Domain.Entities.Product;
using ProductVariantEntity = Domain.Entities.ProductVariant;

namespace Application.Interfaces.Repositories.Product;

public interface IProductUpdateRepository
{
    Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken);
    Task UpdateProductAsync(ProductEntity product, CancellationToken cancellationToken);
    Task RestoreAsync(ProductEntity product, CancellationToken cancellationToken);
    Task RestoreAsync(List<ProductEntity> products, CancellationToken cancellationToken);
    Task AddVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken);
    Task UpdateVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken);
    Task DeleteVariantAsync(ProductVariantEntity variant, CancellationToken cancellationToken);
    Task RemoveVariantAsync(int variantId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
