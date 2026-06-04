using Application.Interfaces.Repositories.InventoryOnHand;
using Infrastructure.DBContexts;
using System.Collections.Generic;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandInsertRepository(ApplicationDBContext context) : IInventoryOnHandInsertRepository
{
    public void Insert(InventoryOnHandEntity inventoryOnHand)
    {
        context.InventoryOnHands.Add(inventoryOnHand);
    }

    public void InsertRange(IEnumerable<InventoryOnHandEntity> inventoryOnHands)
    {
        context.InventoryOnHands.AddRange(inventoryOnHands);
    }
}
