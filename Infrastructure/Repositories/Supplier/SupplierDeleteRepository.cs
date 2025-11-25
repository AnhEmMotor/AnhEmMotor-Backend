using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierDeleteRepository(ApplicationDBContext context) : ISupplierDeleteRepository
{
    public void Delete(SupplierEntity supplier) { context.Suppliers.Remove(supplier); }

    public void Delete(IEnumerable<SupplierEntity> suppliers) { context.Suppliers.RemoveRange(suppliers); }
}