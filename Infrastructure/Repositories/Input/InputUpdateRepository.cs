using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptUpdateRepository(ApplicationDBContext context) : IInventoryReceiptUpdateRepository
{
    public void Update(InventoryReceiptEntity InventoryReceipt)
    {
        context.InventoryReceiptReceipts.Update(InventoryReceipt);
    }

    public void Restore(InventoryReceiptEntity InventoryReceipt)
    {
        context.Restore(InventoryReceipt);
    }

    public void Restore(IEnumerable<InventoryReceiptEntity> InventoryReceipts)
    {
        context.RestoreDeleteUsingSetColumnRange(InventoryReceipts);
    }
}
