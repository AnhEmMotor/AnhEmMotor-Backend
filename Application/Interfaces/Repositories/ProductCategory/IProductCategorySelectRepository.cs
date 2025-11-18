using System.Linq;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory
{
    public interface IProductCategorySelectRepository
    {
        ValueTask<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
        IQueryable<CategoryEntity> GetProductCategories();
        IQueryable<CategoryEntity> GetDeletedProductCategories();
        IQueryable<CategoryEntity> GetAllProductCategories();
        Task<List<CategoryEntity>> GetActiveCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<CategoryEntity>> GetDeletedCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<CategoryEntity>> GetAllCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    }
}
