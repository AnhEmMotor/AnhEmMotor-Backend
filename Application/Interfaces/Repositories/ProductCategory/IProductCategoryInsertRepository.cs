using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryInsertRepository
{
    public void Add(CategoryEntity category);
}
