using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using InputEntity = Domain.Entities.Input;

namespace Infrastructure.Repositories.Input;

public class InputReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IInputReadRepository
{
    public async Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        // Total vehicles: sum of count of products in finished receipts whose InputDate is in the current month
        var totalVehicles = await context.InputReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == Domain.Constants.Input.InputStatus.Finish && x.InputDate >= startOfMonth)
            .SelectMany(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .SumAsync(y => y.Count ?? 0, cancellationToken).ConfigureAwait(false);

        // Processing receipts: count of working receipts
        var processingReceipts = await context.InputReceipts
            .CountAsync(x => x.DeletedAt == null && string.Compare(x.StatusId, Domain.Constants.Input.InputStatus.Working) == 0, cancellationToken).ConfigureAwait(false);

        // Total value: sum of total payable of all finished receipts
        var totalValue = await context.InputReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == Domain.Constants.Input.InputStatus.Finish)
            .SelectMany(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .SumAsync(y => (y.Count ?? 0) * (y.InputPrice ?? 0), cancellationToken).ConfigureAwait(false);

        return new InventoryReceiptStatsResponse
        {
            TotalVehicles = totalVehicles,
            ProcessingReceipts = processingReceipts,
            TotalValue = totalValue
        };
    }

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        Expression<Func<InputEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<InputEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<InputEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.InputReceipts.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }
        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.VariantOptionValues)
            .ThenInclude(x => x.OptionValue)
            .ThenInclude(x => x!.Option)
            .Include(x => x.Supplier)
            .Include(x => x.CreatedByUser)
            .Include(x => x.InputStatus)
            .AsSplitQuery();
    }

    public Task<IEnumerable<InputEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<InputEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InputEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.VariantOptionValues)
            .ThenInclude(x => x.OptionValue)
            .ThenInclude(x => x!.Option)
            .Include(x => x.Supplier)
            .Include(x => x.InputStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<List<InputEntity>> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .ThenInclude(x => x.ProductVariant)
            .ThenInclude(x => x!.Product)
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<InputEntity>> GetBySupplierIdsAsync(
        IEnumerable<int> supplierIds,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(x => x.SupplierId != null && supplierIds.Contains(x.SupplierId.Value))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }
}
