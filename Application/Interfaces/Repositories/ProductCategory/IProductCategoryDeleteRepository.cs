using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryDeleteRepository
{
    public void Delete(CategoryEntity category);

    public void Delete(IEnumerable<CategoryEntity> categories);
}
