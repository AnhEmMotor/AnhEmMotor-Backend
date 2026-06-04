using InventoryOnHandEntity = Domain.Entities.InventoryOnHand;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.InventoryOnHand;

public interface IInventoryOnHandReadRepository
{
    public Task<InventoryOnHandEntity?> GetByVariantAndColorAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken);
    public IQueryable<InventoryOnHandEntity> GetQuery();
}
