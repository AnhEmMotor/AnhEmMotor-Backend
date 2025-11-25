using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierUpdateRepository
    {
        void Update(SupplierEntity supplier);

        void Update(IEnumerable<SupplierEntity> suppliers);

        void Restore(SupplierEntity supplier);

        void Restore(IEnumerable<SupplierEntity> suppliers);
    }
}
