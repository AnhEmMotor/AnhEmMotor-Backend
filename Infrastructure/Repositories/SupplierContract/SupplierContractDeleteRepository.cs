using Application.Interfaces.Repositories.SupplierContract;
using Infrastructure.DBContexts;
using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Infrastructure.Repositories.SupplierContract;

public class SupplierContractDeleteRepository(ApplicationDBContext context) : ISupplierContractDeleteRepository
{
    public void Delete(SupplierContractEntity entity)
    {
        context.SupplierContracts.Remove(entity);
    }

    public void Delete(IEnumerable<SupplierContractEntity> entities)
    {
        context.SupplierContracts.RemoveRange(entities);
    }
}
