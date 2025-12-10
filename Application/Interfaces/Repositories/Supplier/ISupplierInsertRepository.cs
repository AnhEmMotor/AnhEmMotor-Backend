using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierInsertRepository
    {
        public void Add(SupplierEntity supplier);
    }
}
