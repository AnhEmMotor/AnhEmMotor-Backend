using Application.Interfaces.Repositories.Supplier;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierSelectRepository(ApplicationDBContext context) : ISupplierSelectRepository
{
    public Task<SupplierEntity?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Suppliers.FindAsync([id], cancellationToken).AsTask();
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
        return context.Suppliers
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<SupplierEntity>> GetDeletedSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.DeletedOnly<SupplierEntity>()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<SupplierEntity>> GetAllSuppliersByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.All<SupplierEntity>()
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}