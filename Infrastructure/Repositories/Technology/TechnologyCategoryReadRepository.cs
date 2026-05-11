using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Technology;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Technology;

public class TechnologyCategoryReadRepository(ApplicationDBContext context, ISievePaginator paginator) : ITechnologyCategoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<TechnologyCategory, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<TechnologyCategory, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<TechnologyCategory> GetQueryable() => context.TechnologyCategories.AsQueryable();

    public Task<List<TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.TechnologyCategories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
