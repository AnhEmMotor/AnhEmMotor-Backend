using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierDeleteRepository(ApplicationDBContext context) : ISupplierDeleteRepository
    {
        public async Task DeleteSupplierAsync(SupplierEntity supplier)
        {
            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync();
        }

        public async Task DeleteSuppliersAsync(List<SupplierEntity> suppliers)
        {
            context.Suppliers.RemoveRange(suppliers);
            await context.SaveChangesAsync();
        }
    }
}
