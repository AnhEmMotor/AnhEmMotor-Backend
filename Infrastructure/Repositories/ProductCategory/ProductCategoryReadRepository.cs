using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory;

public class ProductCategoryReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IProductCategoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<CategoryEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }
    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .AnyAsync(c => string.Compare(c.Name, name) == 0, cancellationToken);
    }

    public Task<bool> ExistsByNameExceptIdAsync(
        string name,
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .AnyAsync(x => string.Compare(x.Name, name) == 0 && x.Id != id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).ToListAsync(cancellationToken);
    }

    public Task<CategoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken);
    }

    internal IQueryable<CategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode);
    }
}