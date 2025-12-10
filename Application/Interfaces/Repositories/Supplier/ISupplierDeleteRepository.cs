using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierDeleteRepository
    {
        public void Delete(SupplierEntity supplier);

        public void Delete(IEnumerable<SupplierEntity> suppliers);
    }
}
