using Application.Interfaces.Repositories.InventoryOnHand;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Infrastructure.Repositories.InventoryOnHand;

public class InventoryOnHandReadRepository(ApplicationDBContext context) : IInventoryOnHandReadRepository
{
    public async Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken)
    {
        return await context.InventoryOnHands
            .FirstOrDefaultAsync(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId, cancellationToken);
    }

    public IQueryable<InventoryOnHandEntity> GetQuery()
    {
        return context.InventoryOnHands;
    }
}
