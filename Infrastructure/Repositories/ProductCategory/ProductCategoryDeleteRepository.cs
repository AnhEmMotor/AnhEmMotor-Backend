using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory;

public class ProductCategoryDeleteRepository(ApplicationDBContext context) : IProductCategoryDeleteRepository
{
    public void Delete(CategoryEntity category) { context.ProductCategories.Remove(category); }

    public void Delete(IEnumerable<CategoryEntity> categories) { context.ProductCategories.RemoveRange(categories); }
}
