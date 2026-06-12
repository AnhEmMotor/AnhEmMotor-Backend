using Domain.Entities;

namespace Application.Interfaces.Repositories.InventoryReceipt;

public interface IInventoryReceiptInfoReadRepository
{
    Task<List<InventoryReceiptInfo>> GetFinishedInventoryReceiptInfosByVariantIdAsync(int variantId, CancellationToken cancellationToken = default);
}
