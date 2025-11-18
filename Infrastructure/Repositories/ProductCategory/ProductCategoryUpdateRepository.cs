using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryUpdateRepository(ApplicationDBContext context) : IProductCategoryUpdateRepository
    {
        public async Task UpdateAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            context.ProductCategories.Update(category);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreAsync(CategoryEntity category, CancellationToken cancellationToken)
        {
            context.Restore(category);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreAsync(List<CategoryEntity> categories, CancellationToken cancellationToken)
        {
            foreach (var category in categories)
            {
                context.Restore(category);
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
