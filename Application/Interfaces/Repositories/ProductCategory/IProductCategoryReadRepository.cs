using Domain.Constants;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Interfaces.Repositories.ProductCategory;

public interface IProductCategoryReadRepository
{
    public IQueryable<CategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<CategoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);

    public Task<IEnumerable<CategoryEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly);
}
