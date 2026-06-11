using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandDeleteRepository
{
    public void Delete(InventoryOnHandEntity inventoryOnHand);

    public void DeleteRange(IEnumerable<InventoryOnHandEntity> inventoryOnHands);
}
