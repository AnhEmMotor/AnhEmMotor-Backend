using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryUpdateRepository
{
    public void Update(CategoryEntity category);

    public void Restore(CategoryEntity category);

    public void Restore(IEnumerable<CategoryEntity> categories);
}
