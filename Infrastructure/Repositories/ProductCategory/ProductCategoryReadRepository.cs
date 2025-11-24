using Application.Interfaces.Repositories.ProductCategory;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory;

public class ProductCategoryReadRepository(ApplicationDBContext context) : IProductCategoryReadRepository
{
    public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<CategoryEntity>(mode)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<CategoryEntity>(mode)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<CategoryEntity>(mode)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<CategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode);
    }
}