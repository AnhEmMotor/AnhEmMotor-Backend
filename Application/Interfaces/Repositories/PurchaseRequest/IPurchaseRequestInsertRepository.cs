using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Application.Interfaces.Repositories.PurchaseRequest
{
    public interface IPurchaseRequestInsertRepository
    {
        public void Add(PurchaseRequestEntity purchaseRequest);

        public Task InsertAuditLogsAsync(IEnumerable<Domain.Entities.PurchaseRequestAuditLog> logs, CancellationToken ct = default);
        public Task InsertItemAuditLogsAsync(IEnumerable<Domain.Entities.PurchaseRequestItemAuditLog> logs, CancellationToken ct = default);
    }
}
