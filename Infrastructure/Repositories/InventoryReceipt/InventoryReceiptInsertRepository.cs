using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Entities;
using Infrastructure.DBContexts;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptInsertRepository(ApplicationDBContext context) : IInventoryReceiptInsertRepository
{
    public void Add(InventoryReceiptEntity InventoryReceipt)
    {
        context.InventoryReceipts.Add(InventoryReceipt);
    }

    public Task InsertAuditLogsAsync(IEnumerable<InventoryReceiptAuditLog> logs, CancellationToken ct = default)
    {
        context.InventoryReceiptAuditLogs.AddRange(logs);
        return Task.CompletedTask;
    }

    public Task InsertInfoAuditLogsAsync(IEnumerable<InventoryReceiptInfoAuditLog> logs, CancellationToken ct = default)
    {
        context.InventoryReceiptInfoAuditLogs.AddRange(logs);
        return Task.CompletedTask;
    }
}
