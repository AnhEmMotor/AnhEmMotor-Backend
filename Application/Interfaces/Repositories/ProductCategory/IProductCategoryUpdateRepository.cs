using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryUpdateRepository
{
    void Update(CategoryEntity category);

    void Restore(CategoryEntity category);

    void Restore(IEnumerable<CategoryEntity> categories);
}
