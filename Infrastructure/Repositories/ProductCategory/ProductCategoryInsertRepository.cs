using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategoryInsertRepository(ApplicationDBContext context) : IProductCategoryInsertRepository
    {
        public void Add(CategoryEntity category)
        {
            context.ProductCategories.Add(category);
        }
    }
}
