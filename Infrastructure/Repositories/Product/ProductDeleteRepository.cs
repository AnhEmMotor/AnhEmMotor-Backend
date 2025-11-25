using Application.Interfaces.Repositories.Product;
using Infrastructure.DBContexts;
using ProductEntity = Domain.Entities.Product;

namespace Infrastructure.Repositories.Product;

public class ProductDeleteRepository(ApplicationDBContext context) : IProductDeleteRepository
{
    public void Delete(ProductEntity product) { context.SoftDeleteUsingSetColumn(product); }

    public void Delete(IEnumerable<ProductEntity> products) { context.SoftDeleteUsingSetColumnRange(products); }
}
