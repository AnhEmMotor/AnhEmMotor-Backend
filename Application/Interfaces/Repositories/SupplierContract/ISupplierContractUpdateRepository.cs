using SupplierContractEntity = Domain.Entities.SupplierContract;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractUpdateRepository
{
void Update(SupplierContractEntity entity);
void Update(IEnumerable<SupplierContractEntity> entities);
void Restore(SupplierContractEntity entity);
void Restore(IEnumerable<SupplierContractEntity> entities);
}
