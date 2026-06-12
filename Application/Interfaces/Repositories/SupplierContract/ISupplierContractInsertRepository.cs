using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractInsertRepository
{
    void Add(SupplierContractEntity entity);
}
