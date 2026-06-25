using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractDeleteRepository
{
    public void Delete(SupplierContractEntity entity);

    public void Delete(IEnumerable<SupplierContractEntity> entities);
}
