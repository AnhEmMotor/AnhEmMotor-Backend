using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandDeleteRepository(ApplicationDBContext context) : IBrandDeleteRepository
    {
        public async Task DeleteBrandAsync(BrandEntity brand, CancellationToken cancellationToken)
        {
            context.Brands.Remove(brand);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteBrandsAsync(List<BrandEntity> brands, CancellationToken cancellationToken)
        {
            context.Brands.RemoveRange(brands);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
