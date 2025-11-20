using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandSelectRepository(ApplicationDBContext context) : IBrandSelectRepository
{
    public Task<BrandEntity?> GetBrandByIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Brands.FindAsync([id], cancellationToken).AsTask();
    }

    public IQueryable<BrandEntity> GetBrands()
    {
        return context.Brands.AsNoTracking();
    }

    public IQueryable<BrandEntity> GetDeletedBrands()
    {
        return context.DeletedOnly<BrandEntity>().AsNoTracking();
    }

    public IQueryable<BrandEntity> GetAllBrands()
    {
        return context.All<BrandEntity>().AsNoTracking();
    }

    public Task<List<BrandEntity>> GetActiveBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.Brands.Where(b => ids.Contains(b.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<BrandEntity>> GetDeletedBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.DeletedOnly<BrandEntity>().Where(b => ids.Contains(b.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<BrandEntity>> GetAllBrandsByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return context.All<BrandEntity>().Where(b => ids.Contains(b.Id)).ToListAsync(cancellationToken);
    }
}