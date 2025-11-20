using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory
{
    public interface IProductCategoryInsertRepository
    {
        void Add(CategoryEntity category);
    }
}
