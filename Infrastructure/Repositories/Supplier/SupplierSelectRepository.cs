using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierSelectRepository(ApplicationDBContext context) : ISupplierSelectRepository
    {
        public ValueTask<SupplierEntity?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken)
        {
            return context.Suppliers.FindAsync([id, cancellationToken], cancellationToken: cancellationToken);
        }

        public IQueryable<SupplierEntity> GetSuppliers()
        {
            return context.Suppliers.AsNoTracking();
        }

        public IQueryable<SupplierEntity> GetDeletedSuppliers()
        {
            return context.DeletedOnly<SupplierEntity>().AsNoTracking();
        }

        public IQueryable<SupplierEntity> GetAllSuppliers()
        {
            return context.All<SupplierEntity>().AsNoTracking();
        }

        public Task<List<SupplierEntity>> GetActiveSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.Suppliers.Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync(cancellationToken);
        }

        public Task<List<SupplierEntity>> GetDeletedSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.DeletedOnly<SupplierEntity>().Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync(cancellationToken);
        }

        public Task<List<SupplierEntity>> GetAllSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return context.All<SupplierEntity>().Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync(cancellationToken);
        }
    }
}
