using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractInsertRepository
{
    public void Add(SupplierContractEntity entity);
}
