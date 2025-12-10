using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierUpdateRepository
    {
        public void Update(SupplierEntity supplier);

        public void Update(IEnumerable<SupplierEntity> suppliers);

        public void Restore(SupplierEntity supplier);

        public void Restore(IEnumerable<SupplierEntity> suppliers);
    }
}
