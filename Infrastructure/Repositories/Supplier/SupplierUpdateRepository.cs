using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierUpdateRepository(ApplicationDBContext context) : ISupplierUpdateRepository
    {
        public async Task UpdateSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken)
        {
            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken)
        {
            context.Restore(supplier);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreSuppliersAsync(List<SupplierEntity> suppliers, CancellationToken cancellationToken)
        {
            foreach (var supplier in suppliers)
            {
                context.Restore(supplier);
            }
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateSuppliersAsync(List<SupplierEntity> suppliers, CancellationToken cancellationToken)
        {
            context.Suppliers.UpdateRange(suppliers);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
