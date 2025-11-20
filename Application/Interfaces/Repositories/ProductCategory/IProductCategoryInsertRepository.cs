using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory
{
    public interface IProductCategoryInsertRepository
    {
        Task AddAsync(CategoryEntity category, CancellationToken cancellationToken);
    }
}
