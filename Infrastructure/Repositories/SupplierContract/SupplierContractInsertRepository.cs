using Application.Interfaces.Repositories.SupplierContract;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Contract;

public class SupplierContractInsertRepository(ApplicationDBContext context) : ISupplierContractInsertRepository
{
public void Add(Domain.Entities.SupplierContract entity)
{
context.SupplierContracts.Add(entity);
}
}
