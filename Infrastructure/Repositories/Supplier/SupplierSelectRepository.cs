using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier
{
    public class SupplierSelectRepository(ApplicationDBContext context) : ISupplierSelectRepository
    {
        public async Task<SupplierEntity?> GetSupplierByIdAsync(int id)
        {
            return await context.Suppliers.FindAsync(id);
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

        public async Task<List<SupplierEntity>> GetActiveSuppliersByIdsAsync(List<int> ids)
        {
            return await context.Suppliers.Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync();
        }

        public async Task<List<SupplierEntity>> GetDeletedSuppliersByIdsAsync(List<int> ids)
        {
            return await context.DeletedOnly<SupplierEntity>().Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync();
        }

        public async Task<List<SupplierEntity>> GetAllSuppliersByIdsAsync(List<int> ids)
        {
            return await context.All<SupplierEntity>().Where(s => s.Id.HasValue && ids.Contains(s.Id.Value)).ToListAsync();
        }
    }
}
