using Domain.Entities;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestInsertRepository
    {
        public void Add(PurchaseRequestEntity purchaseRequest);

        public Task InsertAuditLogsAsync(IEnumerable<PurchaseRequestAuditLog> logs, CancellationToken ct = default);

        public Task InsertItemAuditLogsAsync(
            IEnumerable<PurchaseRequestItemAuditLog> logs,
            CancellationToken ct = default);
    }
}
