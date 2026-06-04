using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandUpdateRepository
{
    public void Update(InventoryOnHandEntity inventoryOnHand);
    public Task RecalculateAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken);
    public Task RecalculateAllAsync(CancellationToken cancellationToken);
}
