using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptInsertRepository
{
    public void Add(InventoryReceiptEntity InventoryReceipt);
}
