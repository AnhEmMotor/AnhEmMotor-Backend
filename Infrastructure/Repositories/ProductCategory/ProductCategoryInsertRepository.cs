using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryInsertRepository(ApplicationDBContext context) : IProductCategoryInsertRepository
    {
        public async Task<CategoryEntity> AddAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            context.ProductCategories.Add(category);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return category;
        }
    }
}
