using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierInsertRepository
    {
        Task<SupplierEntity> AddSupplierAsync(SupplierEntity supplier, CancellationToken cancellationToken);
    }
}
