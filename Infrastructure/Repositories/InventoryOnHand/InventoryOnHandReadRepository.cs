using Application.Interfaces.Repositories.InventoryOnHand;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandReadRepository(ApplicationDBContext context) : IInventoryOnHandReadRepository
{
    public Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken)
    {
        return context.InventoryOnHands
            .FirstOrDefaultAsync(
                x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId,
                cancellationToken);
    }

    public IQueryable<InventoryOnHandEntity> GetQuery()
    {
        return context.InventoryOnHands;
    }
}
