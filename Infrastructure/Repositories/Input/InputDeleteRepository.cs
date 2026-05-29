using Application.Interfaces.Repositories.InventoryReceipt;
using Infrastructure.DBContexts;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Infrastructure.Repositories.InventoryReceipt;

public class InventoryReceiptDeleteRepository(ApplicationDBContext context) : IInventoryReceiptDeleteRepository
{
    public void Delete(InventoryReceiptEntity InventoryReceipt)
    {
        context.SoftDeleteUsingSetColumn(InventoryReceipt);
    }

    public void Delete(IEnumerable<InventoryReceiptEntity> InventoryReceipts)
    {
        context.SoftDeleteUsingSetColumnRange(InventoryReceipts);
    }

    public void DeleteInventoryReceiptInfo(InventoryReceiptInfoEntity InventoryReceiptInfo)
    {
        context.InventoryReceiptInfos.Remove(InventoryReceiptInfo);
    }
}
