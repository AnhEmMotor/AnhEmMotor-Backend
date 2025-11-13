using SupplierEntity = Domain.Entities.Supplier;

namespace Application.Interfaces.Repositories.Supplier
{
    public interface ISupplierSelectRepository
    {
        Task<SupplierEntity?> GetSupplierByIdAsync(int id);
        IQueryable<SupplierEntity> GetSuppliers();
        IQueryable<SupplierEntity> GetDeletedSuppliers();
        IQueryable<SupplierEntity> GetAllSuppliers();
        Task<List<SupplierEntity>> GetActiveSuppliersByIdsAsync(List<int> ids);
        Task<List<SupplierEntity>> GetDeletedSuppliersByIdsAsync(List<int> ids);
        Task<List<SupplierEntity>> GetAllSuppliersByIdsAsync(List<int> ids);
    }
}
