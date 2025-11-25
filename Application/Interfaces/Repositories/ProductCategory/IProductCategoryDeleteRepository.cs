using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryDeleteRepository
{
    void Delete(CategoryEntity category);

    void Delete(IEnumerable<CategoryEntity> categories);
}
