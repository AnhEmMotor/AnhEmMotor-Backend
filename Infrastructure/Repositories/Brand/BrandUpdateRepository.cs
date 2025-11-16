using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandUpdateRepository(ApplicationDBContext context) : IBrandUpdateRepository
    {
        public async Task UpdateBrandAsync(BrandEntity brand, CancellationToken cancellationToken)
        {
            context.Brands.Update(brand);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreBrandAsync(BrandEntity brand, CancellationToken cancellationToken)
        {
            context.Restore(brand);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task RestoreBrandsAsync(List<BrandEntity> brands, CancellationToken cancellationToken)
        {
            foreach (var brand in brands)
            {
                context.Restore(brand);
            }
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
