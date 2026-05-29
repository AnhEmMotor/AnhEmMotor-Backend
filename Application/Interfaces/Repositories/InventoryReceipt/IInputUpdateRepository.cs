using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptUpdateRepository
{
    public void Update(InventoryReceiptEntity InventoryReceipt);

    public void Restore(InventoryReceiptEntity InventoryReceipt);

    public void Restore(IEnumerable<InventoryReceiptEntity> InventoryReceipts);
}
