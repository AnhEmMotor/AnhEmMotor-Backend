using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ServiceCategory;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Infrastructure.Repositories.ServiceCategory;

public class ServiceCategoryReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator,
    ISieveProcessor sieveProcessor) : IServiceCategoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<ServiceCategoryEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<List<ServiceCategoryEntity>> GetFilteredListAsync(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        var filteredQuery = sieveProcessor.Apply(sieveModel, query, applyPagination: false);
        return filteredQuery.ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<ServiceCategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ServiceCategoryEntity>(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<ServiceCategoryEntity>)t.Result, cancellationToken);
    }

    public Task<ServiceCategoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ServiceCategoryEntity>(mode)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<IEnumerable<ServiceCategoryEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ServiceCategoryEntity>(mode)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<ServiceCategoryEntity>)t.Result, cancellationToken);
    }

    public Task<ServiceCategoryEntity?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // ServiceCategory does not have a Slug field in this schema.
        return Task.FromResult<ServiceCategoryEntity?>(null);
    }

    public Task<IEnumerable<ServiceCategoryEntity>> GetRootCategoriesAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        // ServiceCategory does not have a hierarchy (no ParentId). All are root.
        return GetAllAsync(cancellationToken, mode);
    }

    public Task<IEnumerable<ServiceCategoryEntity>> GetSubCategoriesAsync(
        int parentId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // ServiceCategory does not have a hierarchy (no subcategories).
        return Task.FromResult<IEnumerable<ServiceCategoryEntity>>([]);
    }

    internal IQueryable<ServiceCategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<ServiceCategoryEntity>(mode);
    }
}
