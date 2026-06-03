using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractDeleteRepository
{
    void Delete(SupplierContractEntity entity);
    void Delete(IEnumerable<SupplierContractEntity> entities);
}
