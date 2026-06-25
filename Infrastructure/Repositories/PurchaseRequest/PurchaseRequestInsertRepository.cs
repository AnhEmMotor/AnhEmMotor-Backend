using Application.Interfaces.Repositories.PurchaseRequest;
using Domain.Entities;
using Infrastructure.DBContexts;
using PurchaseRequestEntity = Domain.Entities.PurchaseRequest;

namespace Infrastructure.Repositories.PurchaseRequest
{
    public class PurchaseRequestInsertRepository(ApplicationDBContext context) : IPurchaseRequestInsertRepository
    {
        public void Add(PurchaseRequestEntity purchaseRequest)
        {
            context.PurchaseRequests.Add(purchaseRequest);
        }

        public Task InsertAuditLogsAsync(IEnumerable<PurchaseRequestAuditLog> logs, CancellationToken ct = default)
        {
            context.PurchaseRequestAuditLogs.AddRange(logs);
            return Task.CompletedTask;
        }

        public Task InsertItemAuditLogsAsync(
            IEnumerable<PurchaseRequestItemAuditLog> logs,
            CancellationToken ct = default)
        {
            context.PurchaseRequestItemAuditLogs.AddRange(logs);
            return Task.CompletedTask;
        }
    }
}
