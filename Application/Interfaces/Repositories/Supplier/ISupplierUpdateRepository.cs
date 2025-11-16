using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierUpdateRepository
    {
        Task UpdateSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken);
        Task RestoreSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken);
        Task RestoreSuppliersAsync(List<SupplierEntity> suppliers, CancellationToken cancellationToken);
        Task UpdateSuppliersAsync(List<SupplierEntity> suppliers, CancellationToken cancellationToken);
    }
}
