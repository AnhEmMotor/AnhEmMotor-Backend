using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptInsertRepository
{
    public void Add(InventoryReceiptEntity InventoryReceipt);
    public Task InsertAuditLogsAsync(IEnumerable<Domain.Entities.InventoryReceiptAuditLog> logs, CancellationToken ct = default);
    public Task InsertInfoAuditLogsAsync(IEnumerable<Domain.Entities.InventoryReceiptInfoAuditLog> logs, CancellationToken ct = default);
}
