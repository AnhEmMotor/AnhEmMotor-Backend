using Application.Common.Models;
using Application.Interfaces.Repositories.Technology.Technology;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq.Expressions;
using TechnologyEntity = Domain.Entities.Technology;

namespace Infrastructure.Repositories.Technology.Technology;

public class TechnologyReadRepository(ApplicationDBContext context, ISieveProcessor sieveProcessor)
    : ITechnologyReadRepository
{
    public async Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<TechnologyEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Technologies.AsNoTracking();
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

    public Task<TechnologyEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.Technologies.AsNoTracking();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);

        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<TechnologyEntity>> GetTechnologiesAsync(
        int? categoryId,
        int? brandId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Technologies.AsNoTracking().Where(x => x.DeletedAt == null);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        if (brandId.HasValue)
            query = query.Where(x => x.BrandId == brandId);

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<TechnologyEntity>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        return context.Technologies.Include(x => x.Category).AsNoTracking().Where(x => x.DeletedAt == null).ToListAsync(cancellationToken);
    }
}
