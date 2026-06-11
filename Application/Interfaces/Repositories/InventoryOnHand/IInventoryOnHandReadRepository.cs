using System.Linq;
using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandReadRepository
{
    public Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(
        int productVariantId,
        int? productVariantColorId,
        CancellationToken cancellationToken);

    public IQueryable<InventoryOnHandEntity> GetQuery();
}
