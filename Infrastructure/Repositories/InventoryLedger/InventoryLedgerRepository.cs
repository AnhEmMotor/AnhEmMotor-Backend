using Application.Interfaces.Repositories.InventoryLedger;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.InventoryLedger
{
    public class InventoryLedgerRepository(ApplicationDBContext context) : IInventoryLedgerRepository
    {
        public async Task AddAsync(Domain.Entities.InventoryLedger ledger, CancellationToken cancellationToken)
        {
            await context.InventoryLedgers.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Domain.Entities.InventoryLedger?> GetLastEntryAsync(int productVariantId, int? productVariantColorId, CancellationToken cancellationToken)
        {
            return await context.InventoryLedgers
                .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.InventoryLedger>> GetAllWithDetailsAsync(CancellationToken cancellationToken)
        {
            return await context.InventoryLedgers
                .Include(x => x.ProductVariant)
                    .ThenInclude(v => v!.Product)
                .Include(x => x.ProductVariantColor)
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
