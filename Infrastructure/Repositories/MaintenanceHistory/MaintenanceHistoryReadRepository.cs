using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using MaintenanceHistoryEntity = global::Domain.Entities.MaintenanceHistory;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.MaintenanceHistory;

public class MaintenanceHistoryReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator) : IMaintenanceHistoryReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<MaintenanceHistoryEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null) query = query.Where(filter);
        return paginator.ApplyAsync<MaintenanceHistoryEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<MaintenanceHistoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.Set<MaintenanceHistoryEntity>().IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);
        return query.AsNoTracking();
    }

    public Task<IEnumerable<MaintenanceHistoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode).ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MaintenanceHistoryEntity>>(t => t.Result, cancellationToken);
    }

    public Task<MaintenanceHistoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<MaintenanceHistoryEntity>> GetByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.MaintenanceDate)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<MaintenanceHistoryEntity>>(t => t.Result, cancellationToken);
    }
}
