using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory
{
    public interface IProductCategoryDeleteRepository
    {
        Task DeleteAsync(CategoryEntity category, CancellationToken cancellationToken);
        Task DeleteAsync(List<CategoryEntity> categories, CancellationToken cancellationToken);
    }
}
