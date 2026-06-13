using Application.Interfaces.Repositories.SupplierContract;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.SupplierContract;

public class SupplierContractUpdateRepository(ApplicationDBContext context) : ISupplierContractUpdateRepository
{
    public void Update(Domain.Entities.SupplierContract entity)
    {
        context.SupplierContracts.Update(entity);
    }

    public void Update(IEnumerable<Domain.Entities.SupplierContract> entities)
    {
        context.SupplierContracts.UpdateRange(entities);
    }

    public void Restore(Domain.Entities.SupplierContract entity)
    {
        context.Restore(entity);
    }

    public void Restore(IEnumerable<Domain.Entities.SupplierContract> entities)
    {
        foreach (var entity in entities)
        {
            context.Restore(entity);
        }
    }
}
