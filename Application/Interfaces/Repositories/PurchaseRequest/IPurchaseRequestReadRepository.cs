using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<PagedResult<TResponse>> GetApprovedPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

        public Task<PurchaseRequestEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<PurchaseRequestEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        /// <summary>Tìm Id các yêu cầu mua hàng có ít nhất 1 dòng hàng khớp tên nhà cung cấp (contains, không phân biệt hoa/thường), mới nhất trước.</summary>
        public Task<List<int>> SearchIdsBySupplierNameAsync(
            string keyword,
            int limit,
            CancellationToken cancellationToken);

        public Task<List<PurchaseRequestItem>> GetItemsByIdsAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken);

        public Task<List<PurchaseRequestItem>> GetItemsByPurchaseRequestIdsAsync(
            IEnumerable<int> purchaseRequestIds,
            CancellationToken cancellationToken);

        public Task<List<PurchaseRequestAuditLog>> GetAuditLogsAsync(
            int purchaseRequestId,
            CancellationToken cancellationToken);

        public Task<List<PurchaseRequestItemAuditLog>> GetItemAuditLogsAsync(
            IEnumerable<int> itemIds,
            CancellationToken cancellationToken);

        public Task<List<int>> GetAllItemIdsAsync(int purchaseRequestId, CancellationToken cancellationToken);
    }
}

