using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierInsertRepository(ApplicationDBContext context) : ISupplierInsertRepository
    {
        public async Task<SupplierEntity> AddSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken)
        {
            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return supplier;
        }
    }
}
