using Application.Common.Models;
using Domain.Primitives;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Option;
using Domain.Constants;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using OptionEntity = Domain.Entities.Option;

namespace Infrastructure.Repositories.Option;

public class OptionReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IOptionReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<OptionEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<OptionEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }
    internal IQueryable<OptionEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<OptionEntity>(mode);
    }

    public Task<OptionEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Options.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<List<OptionEntity>> GetByNamesAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var lowerNames = names.Select(n => n.ToLower()).ToList();
        return GetQueryable(mode)
            .Where(o => o.Name != null && lowerNames.Contains(o.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public Task<List<OptionEntity>> GetAllWithOptionsAsync(CancellationToken cancellationToken)
    {
        return context.Options.Include(o => o.OptionValues).AsNoTracking().ToListAsync(cancellationToken);
    }
}
