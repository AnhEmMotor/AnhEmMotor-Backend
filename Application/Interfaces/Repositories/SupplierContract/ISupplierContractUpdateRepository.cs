using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractUpdateRepository
{
    public void Update(SupplierContractEntity entity);
    public void Update(IEnumerable<SupplierContractEntity> entities);
    public void Restore(SupplierContractEntity entity);
    public void Restore(IEnumerable<SupplierContractEntity> entities);
}
