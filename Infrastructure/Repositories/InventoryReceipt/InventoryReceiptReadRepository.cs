using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Infrastructure.Repositories.InventoryReceipt
{
    public class InventoryReceiptReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IInventoryReceiptReadRepository
    {
        public async Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var totalVehicles = await context.InventoryReceiptInfos
                .Where(
                    ii => ii.InventoryReceipt != null &&
                        ii.InventoryReceipt.DeletedAt == null &&
                        ii.InventoryReceipt.StatusId == "approve" &&
                        ii.InventoryReceipt.InventoryReceiptDate >= startOfMonth)
                .SumAsync(ii => ii.Count ?? 0, cancellationToken)
                .ConfigureAwait(false);
            var processingReceipts = await context.InventoryReceipts
                .Where(r => r.DeletedAt == null && r.StatusId == "sent")
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
            return new InventoryReceiptStatsResponse
            {
                TotalVehicles = totalVehicles,
                ProcessingReceipts = processingReceipts
            };
        }

        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<InventoryReceiptEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<InventoryReceiptEntity> query = GetQueryable(mode)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.Vehicles.Where(v => v.DeletedAt == null))
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariantColor)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.InventoryReceiptInfos.Where(rii => rii.DeletedAt == null))
                .ThenInclude(rii => rii.InventoryReceipt)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.CreatedByUser)
                .Include(x => x.SentByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<InventoryReceiptEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        internal IQueryable<InventoryReceiptEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.InventoryReceipts.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            } else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query;
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
            return GetQueryable(mode)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.Vehicles.Where(v => v.DeletedAt == null))
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.ProductVariantColor)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.PurchaseRequestItem)
                .ThenInclude(pri => pri!.InventoryReceiptInfos.Where(rii => rii.DeletedAt == null))
                .ThenInclude(rii => rii.InventoryReceipt)
                .Include(x => x.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.Supplier)
                .Include(x => x.PurchaseRequest)
                .Include(x => x.CreatedByUser)
                .Include(x => x.SentByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<InventoryReceiptEntity>> GetBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryReceiptEntity>> GetBySupplierIdsAsync(
            IEnumerable<int> supplierIds,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            throw new NotImplementedException();
        }

        public Task<InventoryReceiptInfoEntity?> GetInfoByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<InventoryReceiptInfoEntity>> GetInfosByVariantAsync(
            int variantId,
            int? colorId,
            CancellationToken cancellationToken)
        {
            var query = context.InventoryReceiptInfos
                .Where(x => x.DeletedAt == null && x.InventoryReceipt != null && x.InventoryReceipt.DeletedAt == null)
                .Include(x => x.InventoryReceipt)
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseRequestItem)
                .Where(x => x.PurchaseRequestItem != null && x.PurchaseRequestItem.ProductVariantId == variantId);
            if (colorId.HasValue)
            {
                query = query.Where(x => x.PurchaseRequestItem!.ProductVariantColorId == colorId.Value);
            }
            return query.ToListAsync(cancellationToken);
        }

        public async Task<List<Domain.Entities.InventoryReceiptAuditLog>> GetAuditLogsAsync(int inventoryReceiptId, CancellationToken cancellationToken)
        {
            return await context.InventoryReceiptAuditLogs
                .Include(l => l.ChangedBy)
                .Where(l => l.InventoryReceiptId == inventoryReceiptId)
                .OrderByDescending(l => l.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Domain.Entities.InventoryReceiptInfoAuditLog>> GetInfoAuditLogsAsync(int inventoryReceiptId, CancellationToken cancellationToken)
        {
            return await context.InventoryReceiptInfoAuditLogs
                .IgnoreQueryFilters()
                .Include(il => il.InventoryReceiptInfo)
                    .ThenInclude(i => i!.PurchaseRequestItem)
                        .ThenInclude(pri => pri!.ProductVariant)
                            .ThenInclude(pv => pv!.Product)
                .Where(il => il.InventoryReceiptInfo.InventoryReceiptId == inventoryReceiptId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Domain.Entities.VehicleAuditLog>> GetVehicleAuditLogsAsync(int inventoryReceiptId, CancellationToken cancellationToken)
        {
            return await context.VehicleAuditLogs
                .IgnoreQueryFilters()
                .Include(vl => vl.ChangedBy)
                .Include(vl => vl.Vehicle)
                    .ThenInclude(v => v.InventoryReceiptInfo)
                .Where(vl => vl.Vehicle.InventoryReceiptInfo!.InventoryReceiptId == inventoryReceiptId)
                .OrderByDescending(vl => vl.ChangedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
