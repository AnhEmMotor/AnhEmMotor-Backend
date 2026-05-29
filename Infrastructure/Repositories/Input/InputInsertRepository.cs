using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptInsertRepository(ApplicationDBContext context) : IInventoryReceiptInsertRepository
{
    public void Add(InventoryReceiptEntity InventoryReceipt)
    {
        context.InventoryReceiptReceipts.Add(InventoryReceipt);
    }
}
