using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierUpdateRepository
    {
        Task UpdateSupplierAsync(SupplierEntity supplier);
        Task RestoreSupplierAsync(SupplierEntity supplier);
        Task RestoreSuppliersAsync(List<SupplierEntity> suppliers);
        Task UpdateSuppliersAsync(List<SupplierEntity> suppliers);
    }
}
