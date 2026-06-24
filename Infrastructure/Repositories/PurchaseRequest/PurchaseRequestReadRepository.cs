using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Constants;
using Domain.Constants.PurchaseRequest;
using Domain.Entities;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using System.Linq;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IPurchaseRequestReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.InventoryReceipt)
                .AsSplitQuery();
            return paginator.ApplyAsync<PurchaseRequestEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PagedResult<TResponse>> GetApprovedPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.InventoryReceipt)
                .Where(x => x.Status == PurchaseRequestStatus.Approve)
                .Where(
                    x => x.PurchaseRequestItems
                        .Where(item => item.DeletedAt == null)
                        .Any(
                            item => item.Quantity >
                                    item.InventoryReceiptInfos
                                        .Where(
                                            ii => ii.DeletedAt == null &&
                                                            ii.InventoryReceipt != null &&
                                                            ii.InventoryReceipt.DeletedAt == null &&
                                                            string.Compare(
                                                                ii.InventoryReceipt.StatusId,
                                                                Domain.Constants.InventoryReceipt.InventoryReceiptStatus.Approve) ==
                                                            0)
                                        .Sum(ii => ii.Count ?? 0)))
                .AsSplitQuery();
            return paginator.ApplyAsync<PurchaseRequestEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        public Task<PurchaseRequestEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<PurchaseRequestEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode)
                .Include(x => x.CreatedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.ProductCategory)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.ProductVariantColor)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.InventoryReceiptInfos.Where(ii => ii.DeletedAt == null))
                .ThenInclude(ii => ii.InventoryReceipt)
                .Include(x => x.PurchaseRequestItems.Where(item => item.DeletedAt == null))
                .ThenInclude(r => r.Supplier)
                .AsSplitQuery();
            return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        private IQueryable<PurchaseRequestEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.PurchaseRequests.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            } else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query
                .Include(x => x.CreatedByUser)
                .Include(x => x.SentByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser);
        }

        public Task<List<PurchaseRequestItem>> GetItemsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken)
        {
            return context.PurchaseRequestItems
                .Include(x => x.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.ProductCategory)
                .Include(x => x.ProductVariantColor)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }

        public Task<List<PurchaseRequestItem>> GetItemsByPurchaseRequestIdsAsync(
            IEnumerable<int> purchaseRequestIds,
            CancellationToken cancellationToken)
        {
            return context.PurchaseRequestItems
                .IgnoreQueryFilters()
                .Include(x => x.ProductVariant)
                .ThenInclude(pv => pv!.Product)
                .ThenInclude(p => p!.ProductCategory)
                .Include(x => x.ProductVariantColor)
                .Include(x => x.InventoryReceiptInfos)
                .Where(x => purchaseRequestIds.Contains(x.PurchaseRequestId))
                .ToListAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.PurchaseRequestAuditLog>> GetAuditLogsAsync(int purchaseRequestId, CancellationToken cancellationToken)
        {
            return context.PurchaseRequestAuditLogs
                .Include(x => x.ChangedBy)
                .Where(x => x.PurchaseRequestId == purchaseRequestId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.PurchaseRequestItemAuditLog>> GetItemAuditLogsAsync(IEnumerable<int> itemIds, CancellationToken cancellationToken)
        {
            return context.PurchaseRequestItemAuditLogs
                .IgnoreQueryFilters()
                .Include(x => x.PurchaseRequestItem)
                    .ThenInclude(x => x!.ProductVariant)
                        .ThenInclude(x => x!.Product)
                .Where(x => itemIds.Contains(x.PurchaseRequestItemId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public Task<List<int>> GetAllItemIdsAsync(int purchaseRequestId, CancellationToken cancellationToken)
        {
            return context.PurchaseRequestItems
                .IgnoreQueryFilters()
                .Where(x => x.PurchaseRequestId == purchaseRequestId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
