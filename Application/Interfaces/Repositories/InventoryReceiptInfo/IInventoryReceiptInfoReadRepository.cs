using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Application.Interfaces.Repositories.InventoryReceiptInfo;

public interface IInventoryReceiptInfoReadRepository
{
    public Task<List<InventoryReceiptInfoEntity>> GetFinishedInventoryReceiptInfosByVariantIdAsync(
        int variantId,
        CancellationToken cancellationToken = default);
}

