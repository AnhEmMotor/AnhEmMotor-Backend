using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptDeleteRepository
{
    public void Delete(InventoryReceiptEntity InventoryReceipt);

    public void Delete(IEnumerable<InventoryReceiptEntity> InventoryReceipts);

    public void DeleteInventoryReceiptInfo(InventoryReceiptInfoEntity InventoryReceiptInfo);
}
