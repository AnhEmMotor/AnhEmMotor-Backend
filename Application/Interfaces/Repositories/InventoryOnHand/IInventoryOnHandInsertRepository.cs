using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandInsertRepository
{
    public void Insert(InventoryOnHandEntity inventoryOnHand);

    public void InsertRange(IEnumerable<InventoryOnHandEntity> inventoryOnHands);
}
