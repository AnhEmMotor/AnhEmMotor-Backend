using Application.Interfaces.Repositories.ProductCategory;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory
{
    public class ProductCategorySelectRepository(ApplicationDBContext context) : IProductCategorySelectRepository
    {
        public ValueTask<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.ProductCategories.FindAsync([id], cancellationToken: cancellationToken);
        }

        public IQueryable<CategoryEntity> GetProductCategories()
        {
            return context.ProductCategories.AsNoTracking();
        }

        public IQueryable<CategoryEntity> GetDeletedProductCategories()
        {
            return context.DeletedOnly<CategoryEntity>().AsNoTracking();
        }

        public IQueryable<CategoryEntity> GetAllProductCategories()
        {
            return context.All<CategoryEntity>().AsNoTracking();
        }

        public Task<List<CategoryEntity>> GetActiveCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.ProductCategories.Where(category => category.Id.HasValue && ids.Contains(category.Id.Value)).ToListAsync(cancellationToken);
        }

        public Task<List<CategoryEntity>> GetDeletedCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.DeletedOnly<CategoryEntity>().Where(category => category.Id.HasValue && ids.Contains(category.Id.Value)).ToListAsync(cancellationToken);
        }

        public Task<List<CategoryEntity>> GetAllCategoriesByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.All<CategoryEntity>().Where(category => category.Id.HasValue && ids.Contains(category.Id.Value)).ToListAsync(cancellationToken);
        }
    }
}
