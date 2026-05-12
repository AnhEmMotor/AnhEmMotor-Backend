using Application.Interfaces.Repositories.TechnologyCategory.TechnologyCategory;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.TechnologyCategory.TechnologyCategory;

public class TechnologyCategoryReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor) : ITechnologyCategoryReadRepository
{
    public async Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.TechnologyCategory, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.TechnologyCategories.AsNoTracking();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);
        if (filter != null)
            query = query.Where(filter);
        var totalItems = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        query = sieveProcessor.Apply(sieveModel, query);
        var items = await query.ProjectToType<TResponse>().ToListAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResult<TResponse>(items, totalItems, sieveModel.Page ?? 1, sieveModel.PageSize ?? 10);
    }

    public Task<List<Domain.Entities.TechnologyCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.TechnologyCategories
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
}
