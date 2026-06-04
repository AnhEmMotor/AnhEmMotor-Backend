using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;
using System.Collections.Generic;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandInsertRepository
{
    public void Insert(InventoryOnHandEntity inventoryOnHand);
    public void InsertRange(IEnumerable<InventoryOnHandEntity> inventoryOnHands);
}
