using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierUpdateRepository(ApplicationDBContext context) : ISupplierUpdateRepository
    {
        public async Task UpdateSupplierAsync(SupplierEntity supplier)
        {
            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync();
        }

        public async Task RestoreSupplierAsync(SupplierEntity supplier)
        {
            context.Restore(supplier);
            await context.SaveChangesAsync();
        }

        public async Task RestoreSuppliersAsync(List<SupplierEntity> suppliers)
        {
            foreach (var supplier in suppliers)
            {
                context.Restore(supplier);
            }
            await context.SaveChangesAsync();
        }

        public async Task UpdateSuppliersAsync(List<SupplierEntity> suppliers)
        {
            context.Suppliers.UpdateRange(suppliers);
            await context.SaveChangesAsync();
        }
    }
}
