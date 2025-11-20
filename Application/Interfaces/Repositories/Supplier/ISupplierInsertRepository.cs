using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierInsertRepository
    {
        void Add(SupplierEntity supplier);
    }
}
