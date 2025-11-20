using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierInsertRepository(ApplicationDBContext context) : ISupplierInsertRepository
{
    public async Task AddAsync(SupplierEntity supplier, CancellationToken cancellationToken)
    {
        await context.Suppliers.AddAsync(supplier, cancellationToken).ConfigureAwait(false);
    }
}