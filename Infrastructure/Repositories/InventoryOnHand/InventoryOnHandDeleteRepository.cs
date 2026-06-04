using Application.Interfaces.Repositories.InventoryOnHand;
using Infrastructure.DBContexts;
using System.Collections.Generic;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandDeleteRepository(ApplicationDBContext context) : IInventoryOnHandDeleteRepository
{
    public void Delete(InventoryOnHandEntity inventoryOnHand)
    {
        context.SoftDeleteUsingSetColumn(inventoryOnHand);
    }

    public void DeleteRange(IEnumerable<InventoryOnHandEntity> inventoryOnHands)
    {
        context.SoftDeleteUsingSetColumnRange(inventoryOnHands);
    }
}
