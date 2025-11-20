using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryInsertRepository(ApplicationDBContext context) : IProductCategoryInsertRepository
    {
        public async Task AddAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            await context.ProductCategories.AddAsync(category, cancellationToken).ConfigureAwait(false);
        }
    }
}
