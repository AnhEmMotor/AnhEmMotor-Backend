using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology;

public class TechnologyReadRepository(ApplicationDBContext context, ISievePaginator paginator) : ITechnologyReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<TechnologyEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<TechnologyEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }
    internal IQueryable<TechnologyEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        IQueryable<TechnologyEntity> query = context.Technologies;
        if (mode != DataFetchMode.ActiveOnly)
            query = query.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(t => t.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(t => t.DeletedAt != null);
        }
        return query;
    }

    public Task<TechnologyEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task<List<TechnologyEntity>> GetTechnologiesAsync(
        int? categoryId,
        int? brandId,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable()
            .Include(t => t.Category)
            .AsQueryable();
            
        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value || t.CategoryId == null);
        }
        if (brandId.HasValue)
        {
            query = query.Where(t => t.BrandId == brandId.Value || t.BrandId == null);
        }
        
        return query
            .OrderBy(t => t.CategoryId)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<List<TechnologyEntity>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        return context.Technologies
            .Include(t => t.Category)
            .ToListAsync(cancellationToken);
    }
}
