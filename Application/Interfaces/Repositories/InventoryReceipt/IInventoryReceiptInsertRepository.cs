using Domain.Entities;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptInsertRepository
{
    public void Add(InventoryReceiptEntity InventoryReceipt);

    public Task InsertAuditLogsAsync(IEnumerable<InventoryReceiptAuditLog> logs, CancellationToken ct = default);

    public Task InsertInfoAuditLogsAsync(IEnumerable<InventoryReceiptInfoAuditLog> logs, CancellationToken ct = default);
}
