using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IVehicleReadRepository
{
    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<Domain.Entities.Vehicle, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<Domain.Entities.Vehicle, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<Domain.Entities.Vehicle> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<Domain.Entities.Vehicle>(mode).Include(v => v.Lead);
    }

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(DataFetchMode.ActiveOnly).Include(v => v.Lead).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(
                v => v.LicensePlate.Contains(search) ||
                    v.VinNumber.Contains(search) ||
                    v.Lead!.FullName.Contains(search));
        }
        return query
            .OrderByDescending(v => v.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return context.Vehicles.Include(v => v.Lead).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public Task<List<Domain.Entities.Vehicle>> GetByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return context.Vehicles
            .Include(v => v.Lead)
            .Include(v => v.InventoryReceiptInfo)
            .Include(v => v.OutputInfo)
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesForAssignmentAsync(
        IEnumerable<int> productVariantIds,
        CancellationToken cancellationToken = default)
    {
        var ids = productVariantIds.Distinct().ToList();
        return context.Vehicles
            .Include(v => v.InventoryReceiptInfo)
            .Include(v => v.OutputInfo)
            .Where(v => v.ProductVariantId.HasValue && ids.Contains(v.ProductVariantId.Value))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByVinAsync(string vin, CancellationToken cancellationToken = default)
    {
        return context.Vehicles.AnyAsync(v => string.Compare(v.VinNumber, vin) == 0, cancellationToken);
    }

    public Task<bool> ExistsByEngineNumberAsync(string engineNumber, CancellationToken cancellationToken = default)
    {
        return context.Vehicles.AnyAsync(v => string.Compare(v.EngineNumber, engineNumber) == 0, cancellationToken);
    }

    public Task<bool> ExistsByVinAsync(
        string vin,
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken = default)
    {
        return context.Vehicles
            .AnyAsync(
                v => string.Compare(v.VinNumber, vin) == 0 &&
                    v.ProductVariantId == productVariantId &&
                    v.ProductVariantColorId == productVariantColorId,
                cancellationToken);
    }

    public Task<bool> ExistsByEngineNumberAsync(
        string engineNumber,
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken = default)
    {
        return context.Vehicles
            .AnyAsync(
                v => string.Compare(v.EngineNumber, engineNumber) == 0 &&
                    v.ProductVariantId == productVariantId &&
                    v.ProductVariantColorId == productVariantColorId,
                cancellationToken);
    }

    public Task<List<Domain.Entities.Vehicle>> GetVehiclesByReceiptInfoIdAsync(
        int receiptInfoId,
        CancellationToken cancellationToken = default)
    {
        return context.Vehicles.Where(v => v.InventoryReceiptInfoId == receiptInfoId).ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default)
    {
        return context.Vehicles.FirstOrDefaultAsync(v => string.Compare(v.VinNumber, vin) == 0, cancellationToken);
    }

    public Task<List<Domain.Entities.Vehicle>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(userId, out var parsedUserId))
        {
            return context.Vehicles.Where(v => v.UserId == parsedUserId).ToListAsync(cancellationToken);
        }
        return Task.FromResult(new List<Domain.Entities.Vehicle>());
    }

    public Task<List<Domain.Entities.Vehicle>> GetByLeadIdAsync(
        int leadId,
        CancellationToken cancellationToken = default)
    {
        return context.Vehicles
            .Include(v => v.Lead)
            .Where(v => v.LeadId == leadId)
            .OrderByDescending(v => v.PurchaseDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Domain.Entities.Vehicle?> GetByLicensePlateAsync(string licensePlate, CancellationToken cancellationToken = default)
    {
        return context.Vehicles
            .Include(v => v.User)
            .Include(v => v.ProductVariantColor)
            .Include(v => v.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                    .ThenInclude(p => p!.Brand)
            .FirstOrDefaultAsync(v => string.Compare(v.LicensePlate, licensePlate) == 0, cancellationToken);
    }
}
