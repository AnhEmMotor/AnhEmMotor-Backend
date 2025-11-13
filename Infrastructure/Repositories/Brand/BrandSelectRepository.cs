using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandSelectRepository(ApplicationDBContext context) : IBrandSelectRepository
    {
        public async Task<BrandEntity?> GetBrandByIdAsync(int id)
        {
            return await context.Brands.FindAsync(id);
        }

        public IQueryable<BrandEntity> GetBrands()
        {
            return context.Brands.AsNoTracking();
        }

        public IQueryable<BrandEntity> GetDeletedBrands()
        {
            return context.DeletedOnly<BrandEntity>().AsNoTracking();
        }

        public async Task<List<BrandEntity>> GetActiveBrandsByIdsAsync(List<int> ids)
        {
            return await context.Brands.Where(b => b.Id.HasValue && ids.Contains(b.Id.Value)).ToListAsync();
        }

        public async Task<List<BrandEntity>> GetDeletedBrandsByIdsAsync(List<int> ids)
        {
            return await context.DeletedOnly<BrandEntity>().Where(b => b.Id.HasValue && ids.Contains(b.Id.Value)).ToListAsync();
        }
    }
}
