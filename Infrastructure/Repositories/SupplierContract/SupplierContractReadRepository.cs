using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Infrastructure.Repositories.SupplierContract;

public class SupplierContractReadRepository(ApplicationDBContext context, ISievePaginator paginator) : ISupplierContractReadRepository
{
    internal IQueryable<SupplierContractEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierContractEntity>(mode)
            .Include(sc => sc.Supplier)
            .Include(sc => sc.ContractItems)
            .ThenInclude(ci => ci.ProductVariant)
            .ThenInclude(pv => pv!.Product)
            .ThenInclude(p => p!.ProductCategory)
            .Include(sc => sc.AuditLogs);
    }

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<SupplierContractEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<SupplierContractEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<SupplierContractEntity>> GetAllAsync(
        CancellationToken cancellationToken = default,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryable(mode).ToListAsync(cancellationToken);
    }

    public Task<SupplierContractEntity?> GetActiveContractBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.ActiveOnly)
            .FirstOrDefaultAsync(
                x => x.SupplierId == supplierId &&
                    string.Compare(x.Status, SupplierContractStatus.Active) == 0 &&
                    (!x.ExpirationDate.HasValue || x.ExpirationDate.Value > DateTime.Now),
                cancellationToken);
    }

    public Task<List<SupplierContractAuditLog>> GetAuditLogsAsync(
        Guid supplierContractId,
        CancellationToken cancellationToken = default)
    {
        return context.SupplierContractAuditLogs
            .IgnoreQueryFilters()
            .Where(al => al.SupplierContractId == supplierContractId)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsContractNumberExistsAsync(
        string contractNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.ContractNumber, contractNumber) == 0 &&
                    (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.ActiveOnly).CountAsync(cancellationToken);
    }

    public Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.ActiveOnly)
            .CountAsync(x => string.Compare(x.Status, status) == 0, cancellationToken);
    }

    public Task<int> CountExpiringAsync(int daysThreshold, CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.Now.AddDays(daysThreshold);
        return GetQueryable(DataFetchMode.ActiveOnly)
            .CountAsync(
                x => string.Compare(x.Status, SupplierContractStatus.Active) == 0 &&
                    x.ExpirationDate.HasValue &&
                    x.ExpirationDate.Value <= thresholdDate &&
                    x.ExpirationDate.Value >= DateTime.Now,
                cancellationToken);
    }
}
