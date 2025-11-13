using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandUpdateRepository(ApplicationDBContext context) : IBrandUpdateRepository
    {
        public async Task UpdateBrandAsync(BrandEntity brand)
        {
            context.Brands.Update(brand);
            await context.SaveChangesAsync();
        }

        public async Task RestoreBrandAsync(BrandEntity brand)
        {
            context.Restore(brand);
            await context.SaveChangesAsync();
        }

        public async Task RestoreBrandsAsync(List<BrandEntity> brands)
        {
            foreach (var brand in brands)
            {
                context.Restore(brand);
            }
            await context.SaveChangesAsync();
        }
    }
}
