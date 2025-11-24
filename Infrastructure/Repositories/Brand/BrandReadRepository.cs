using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandReadRepository(ApplicationDBContext context) : IBrandReadRepository
{
    public async Task<IEnumerable<BrandEntity>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<BrandEntity>(mode)
            .ToListAsync(cancellationToken);
    }

    public async Task<BrandEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<BrandEntity>(mode)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<BrandEntity>> GetByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return await context.GetQuery<BrandEntity>(mode)
            .Where(b => ids.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public IQueryable<BrandEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<BrandEntity>(mode);
    }
}