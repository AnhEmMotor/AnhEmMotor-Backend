using Application.ApiContracts.Input.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Domain.Constants;
using Domain.Constants.Input;
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
        var totalVehicles = await context.InputReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == InputStatus.Finish && x.InputDate >= startOfMonth)
            .SelectMany(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .SumAsync(y => y.Count ?? 0, cancellationToken)
            .ConfigureAwait(false);
        var processingReceipts = await context.InputReceipts
            .CountAsync(
                x => x.DeletedAt == null && string.Compare(x.StatusId, InputStatus.Working) == 0,
                cancellationToken)
            .ConfigureAwait(false);
        var totalValue = await context.InputReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == InputStatus.Finish)
            .SelectMany(x => x.InputInfos.Where(y => y.DeletedAt == null))
            .SumAsync(y => (long)(y.Count ?? 0) * (long)(y.QuotationProductRow != null ? (y.QuotationProductRow.QuotePrice ?? 0) : 0), cancellationToken)
            .ConfigureAwait(false);
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
                .ThenInclude(x => x.Vehicles)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.QuotationReceipt)
                        .ThenInclude(x => x!.Supplier)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.VariantOptionValues)
                            .ThenInclude(x => x.OptionValue)
                                .ThenInclude(x => x!.Option)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariantColor)
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
                .ThenInclude(x => x.Vehicles)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.QuotationReceipt)
                        .ThenInclude(x => x!.Supplier)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.VariantOptionValues)
                            .ThenInclude(x => x.OptionValue)
                                .ThenInclude(x => x!.Option)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InputInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariantColor)
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
            .Where(x => x.InputInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId == supplierId))
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
            .Where(x => x.InputInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId != null && supplierIds.Contains(ii.QuotationProductRow.QuotationReceipt.SupplierId.Value)))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InputEntity>>(t => t.Result, cancellationToken);
    }
}
