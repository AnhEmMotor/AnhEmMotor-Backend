using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq.Expressions;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IInventoryReceiptReadRepository
{
    public async Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var totalVehicles = await context.InventoryReceiptReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == InventoryReceiptStatus.Approve && x.InventoryReceiptDate >= startOfMonth)
            .SelectMany(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
            .SumAsync(y => y.Count ?? 0, cancellationToken)
            .ConfigureAwait(false);
        var processingReceipts = await context.InventoryReceiptReceipts
            .CountAsync(
                x => x.DeletedAt == null && (string.Compare(x.StatusId, InventoryReceiptStatus.Draft) == 0 || string.Compare(x.StatusId, InventoryReceiptStatus.Sent) == 0),
                cancellationToken)
            .ConfigureAwait(false);
        var totalValue = await context.InventoryReceiptReceipts
            .Where(x => x.DeletedAt == null && x.StatusId == InventoryReceiptStatus.Approve)
            .SelectMany(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
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
        Expression<Func<InventoryReceiptEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        if (filter != null)
        {
            query = query.Where(filter);
        }
        return paginator.ApplyAsync<InventoryReceiptEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    internal IQueryable<InventoryReceiptEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = context.InventoryReceiptReceipts.IgnoreQueryFilters();
        if (mode == DataFetchMode.ActiveOnly)
        {
            query = query.Where(x => x.DeletedAt == null);
        } else if (mode == DataFetchMode.DeletedOnly)
        {
            query = query.Where(x => x.DeletedAt != null);
        }
        return query
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.Vehicles)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.QuotationReceipt)
                        .ThenInclude(x => x!.Supplier)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.VariantOptionValues)
                            .ThenInclude(x => x.OptionValue)
                                .ThenInclude(x => x!.Option)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.CreatedByUser)
            .Include(x => x.InventoryReceiptStatus)
            .AsSplitQuery();
    }

    public Task<IEnumerable<InventoryReceiptEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InventoryReceiptEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InventoryReceiptEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<IEnumerable<InventoryReceiptEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InventoryReceiptEntity>>(t => t.Result, cancellationToken);
    }

    public Task<InventoryReceiptEntity?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.Vehicles)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.QuotationReceipt)
                        .ThenInclude(x => x!.Supplier)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.VariantOptionValues)
                            .ThenInclude(x => x.OptionValue)
                                .ThenInclude(x => x!.Option)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.QuotationProductRow)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
            .Include(x => x.InventoryReceiptInfos.Where(y => y.DeletedAt == null))
                .ThenInclude(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariantColor)
            .Include(x => x.InventoryReceiptStatus)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ContinueWith(t => t.Result, cancellationToken);
    }

    public Task<List<InventoryReceiptEntity>> GetBySupplierIdAsync(
        int supplierId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(x => x.InventoryReceiptInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId == supplierId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<InventoryReceiptEntity>> GetBySupplierIdsAsync(
        IEnumerable<int> supplierIds,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var query = GetQueryable(mode);
        return query
            .Where(x => x.InventoryReceiptInfos.Any(ii => ii.QuotationProductRow != null && ii.QuotationProductRow.QuotationReceipt != null && ii.QuotationProductRow.QuotationReceipt.SupplierId != null && supplierIds.Contains(ii.QuotationProductRow.QuotationReceipt.SupplierId.Value)))
            .ToListAsync(cancellationToken)
            .ContinueWith<IEnumerable<InventoryReceiptEntity>>(t => t.Result, cancellationToken);
    }
}
