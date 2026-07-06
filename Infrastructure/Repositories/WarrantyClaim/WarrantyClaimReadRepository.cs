using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using WarrantyClaimEntity = global::Domain.Entities.WarrantyClaim;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.WarrantyClaim;

public class WarrantyClaimReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator) : IWarrantyClaimReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<global::Domain.Entities.WarrantyClaim, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null) query = query.Where(filter);
        return paginator.ApplyAsync<global::Domain.Entities.WarrantyClaim, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<global::Domain.Entities.WarrantyClaim> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.Set<global::Domain.Entities.WarrantyClaim>().IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);
        return query.AsNoTracking();
    }

    public Task<IEnumerable<global::Domain.Entities.WarrantyClaim>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode).ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<global::Domain.Entities.WarrantyClaim>>(t => t.Result, cancellationToken);
    }

    public Task<global::Domain.Entities.WarrantyClaim?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<global::Domain.Entities.WarrantyClaim>> GetByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<global::Domain.Entities.WarrantyClaim>>(t => t.Result, cancellationToken);
    }
}
