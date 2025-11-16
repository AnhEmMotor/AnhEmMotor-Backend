using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierDeleteRepository(ApplicationDBContext context) : ISupplierDeleteRepository
    {
        public async Task DeleteSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken)
        {
            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteSuppliersAsync(List<SupplierEntity> suppliers, CancellationToken cancellationToken)
        {
            context.Suppliers.RemoveRange(suppliers);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
