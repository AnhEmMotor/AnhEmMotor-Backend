using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierUpdateRepository
    {
        void Update(SupplierEntity supplier);
        void Update(List<SupplierEntity> suppliers);
        void Restore(SupplierEntity supplier);
        void Restore(List<SupplierEntity> suppliers);
    }
}
