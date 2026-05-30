using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.InventoryLedger
{
    public interface IInventoryLedgerRepository
    {
        Task AddAsync(Domain.Entities.InventoryLedger ledger, CancellationToken cancellationToken);

        Task<Domain.Entities.InventoryLedger?> GetLastEntryAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken);

        Task<List<Domain.Entities.InventoryLedger>> GetAllWithDetailsAsync(CancellationToken cancellationToken);
    }
}
