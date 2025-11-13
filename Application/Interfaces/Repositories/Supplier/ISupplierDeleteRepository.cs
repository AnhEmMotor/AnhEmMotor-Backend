using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierDeleteRepository
    {
        Task DeleteSupplierAsync(SupplierEntity supplier);
        Task DeleteSuppliersAsync(List<SupplierEntity> suppliers);
    }
}
