using Application.Interfaces.Repositories.InventoryReceiptInfo;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace Infrastructure.Repositories.InventoryReceiptInfo;

public class InventoryReceiptInfoReadRepository(ApplicationDBContext context) : IInventoryReceiptInfoReadRepository
{
    public Task<List<InventoryReceiptInfoEntity>> GetFinishedInventoryReceiptInfosByVariantIdAsync(int variantId, CancellationToken cancellationToken = default)
        => context.InventoryReceiptInfos
            .Include(ii => ii.InventoryReceipt)
            .Where(ii => ii.PurchaseRequestItem != null &&
                         ii.PurchaseRequestItem.ProductVariantId == variantId &&
                         ii.InventoryReceipt != null &&
                         ii.InventoryReceipt.StatusId == "finished")
            .ToListAsync(cancellationToken);
}
