using Application.Interfaces.Repositories.InventoryLedger;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Repositories.InventoryLedger
{
    public class InventoryLedgerRepository(ApplicationDBContext context) : IInventoryLedgerRepository
    {
        public async Task AddAsync(Domain.Entities.InventoryLedger ledger, CancellationToken cancellationToken)
        {
            await context.InventoryLedgers.AddAsync(ledger, cancellationToken).ConfigureAwait(false);
        }

        public Task<Domain.Entities.InventoryLedger?> GetLastEntryAsync(
            int productVariantId,
            int? productVariantColorId,
            CancellationToken cancellationToken)
        {
            return context.InventoryLedgers
                .Where(x => x.ProductVariantId == productVariantId && x.ProductVariantColorId == productVariantColorId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<List<Domain.Entities.InventoryLedger>> GetAllWithDetailsAsync(CancellationToken cancellationToken)
        {
            return context.InventoryLedgers
                .Include(x => x.ProductVariant)
                .ThenInclude(v => v!.Product)
                .Include(x => x.ProductVariantColor)
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
