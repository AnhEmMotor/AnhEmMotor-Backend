using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.WorkshopPayment;

public class WorkshopPaymentReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator) : IWorkshopPaymentReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<global::Domain.Entities.WorkshopPayment, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null) query = query.Where(filter);
        return paginator.ApplyAsync<global::Domain.Entities.WorkshopPayment, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<global::Domain.Entities.WorkshopPayment> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.Set<global::Domain.Entities.WorkshopPayment>().IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);
        return query.AsNoTracking();
    }

    public Task<IEnumerable<global::Domain.Entities.WorkshopPayment>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode).ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<global::Domain.Entities.WorkshopPayment>>(t => t.Result, cancellationToken);
    }

    public Task<global::Domain.Entities.WorkshopPayment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<global::Domain.Entities.WorkshopPayment>> GetBySourceAsync(
        string sourceType,
        int sourceId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<global::Domain.Entities.WorkshopPayment>>(t => t.Result, cancellationToken);
    }
}
