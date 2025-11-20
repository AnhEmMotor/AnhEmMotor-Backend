using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryUpdateRepository(ApplicationDBContext context) : IProductCategoryUpdateRepository
    {
        public void Update(CategoryEntity category)
        {
            context.ProductCategories.Update(category);
        }

        public void Restore(CategoryEntity category)
        {
            context.Restore(category);
        }

        public void Restore(List<CategoryEntity> categories)
        {
            foreach (var category in categories)
            {
                context.Restore(category);
            }
        }
    }
}
