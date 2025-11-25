using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierDeleteRepository
    {
        void Delete(SupplierEntity supplier);

        void Delete(IEnumerable<SupplierEntity> suppliers);
    }
}
