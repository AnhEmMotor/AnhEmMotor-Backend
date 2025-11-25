using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierUpdateRepository(ApplicationDBContext context) : ISupplierUpdateRepository
{
    public void Update(SupplierEntity supplier) { context.Suppliers.Update(supplier); }

    public void Update(IEnumerable<SupplierEntity> suppliers) { context.Suppliers.UpdateRange(suppliers); }

    public void Restore(SupplierEntity supplier) { context.Restore(supplier); }

    public void Restore(IEnumerable<SupplierEntity> suppliers)
    {
        foreach(var supplier in suppliers)
        {
            context.Restore(supplier);
        }
    }
}