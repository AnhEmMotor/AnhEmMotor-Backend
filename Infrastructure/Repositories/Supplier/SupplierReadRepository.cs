using Application.Interfaces.Repositories.Supplier;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierReadRepository(ApplicationDBContext context) : ISupplierReadRepository
{
    public IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode);
    }

    public async Task<IEnumerable<SupplierEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<SupplierEntity>(mode)
            .ToListAsync(cancellationToken);
    }

    public async Task<SupplierEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<SupplierEntity>(mode)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SupplierEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<SupplierEntity>(mode)
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }
}