using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory
{
    public interface IProductCategoryUpdateRepository
    {
        Task UpdateAsync(CategoryEntity category, CancellationToken cancellationToken);
        Task RestoreAsync(CategoryEntity category, CancellationToken cancellationToken);
        Task RestoreAsync(List<CategoryEntity> categories, CancellationToken cancellationToken);
    }
}
