using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using WarrantyClaimEntity = global::Domain.Entities.WarrantyClaim;

namespace Infrastructure.Repositories.WarrantyClaim;

public class WarrantyClaimReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IWarrantyClaimReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<WarrantyClaimEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
            query = query.Where(filter);
        return paginator.ApplyAsync<WarrantyClaimEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<WarrantyClaimEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.Set<WarrantyClaimEntity>().IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
            query = query.Where(x => x.DeletedAt == null);
        else if (mode == DataFetchMode.DeletedOnly)
            query = query.Where(x => x.DeletedAt != null);
        return query.AsNoTracking();
    }

    public Task<IEnumerable<WarrantyClaimEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<WarrantyClaimEntity>>(t => t.Result, cancellationToken);
    }

    public async Task<IEnumerable<WarrantyClaimEntity>> GetAllWithDetailsAsync(
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var claims = await GetQueryable(mode)
            .Include(c => c.WarrantyClaimParts)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (!claims.Any())
            return claims;
        var vehicleIds = claims.Select(c => c.VehicleId).Distinct().ToList();
        var vehicles = await context.Set<Domain.Entities.Vehicle>()
            .Include(v => v.Lead)
            .Where(v => vehicleIds.Contains(v.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var vehicleLookup = vehicles.ToDictionary(v => v.Id);
        foreach (var claim in claims)
        {
            if (vehicleLookup.TryGetValue(claim.VehicleId, out var vehicle))
            {
                claim.Vehicle = vehicle;
            }
        }
        return claims;
    }

    public Task<WarrantyClaimEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public async Task<WarrantyClaimEntity?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode).Include(c => c.WarrantyClaimParts);
        var claim = await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
        if (claim == null)
            return null;
        claim.Vehicle = await context.Set<Domain.Entities.Vehicle>()
            .Include(v => v.Lead)
            .FirstOrDefaultAsync(v => v.Id == claim.VehicleId, cancellationToken)
            .ConfigureAwait(false);
        return claim;
    }

    public Task<IEnumerable<WarrantyClaimEntity>> GetByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<WarrantyClaimEntity>>(t => t.Result, cancellationToken);
    }

    public async Task<IEnumerable<WarrantyClaimEntity>> GetHistoryByVehicleIdAsync(
        int vehicleId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await GetQueryable(mode)
            .Include(c => c.WarrantyClaimParts)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
