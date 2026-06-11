using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandUpdateRepository
{
    public void Update(InventoryOnHandEntity inventoryOnHand);

    public Task RecalculateAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken);

    public Task RecalculateAllAsync(CancellationToken cancellationToken);
}
