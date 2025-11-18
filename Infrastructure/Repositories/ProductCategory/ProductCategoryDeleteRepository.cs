using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryDeleteRepository(ApplicationDBContext context) : IProductCategoryDeleteRepository
    {
        public async Task DeleteAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            context.ProductCategories.Remove(category);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(List<CategoryEntity> categories, CancellationToken cancellationToken)
        {
            context.ProductCategories.RemoveRange(categories);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
