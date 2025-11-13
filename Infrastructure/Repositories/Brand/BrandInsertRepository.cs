using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandInsertRepository(ApplicationDBContext context) : IBrandInsertRepository
    {
        public async Task<BrandEntity> AddBrandAsync(BrandEntity brand)
        {
            context.Brands.Add(brand);
            await context.SaveChangesAsync();
            return brand;
        }
    }
}
