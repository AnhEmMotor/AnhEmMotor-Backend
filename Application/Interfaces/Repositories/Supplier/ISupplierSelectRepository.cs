using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierSelectRepository
    {
        Task<SupplierEntity?> GetByIdAsync(int id, CancellationToken cancellationToken);
        IQueryable<SupplierEntity> GetSuppliers();
        IQueryable<SupplierEntity> GetDeletedSuppliers();
        IQueryable<SupplierEntity> GetAllSuppliers();
        Task<List<SupplierEntity>> GetActiveSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<SupplierEntity>> GetDeletedSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken);
        Task<List<SupplierEntity>> GetAllSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken);
    }
}
