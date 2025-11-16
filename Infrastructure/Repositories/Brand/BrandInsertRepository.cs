using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand
{
    public class BrandInsertRepository(ApplicationDBContext context) : IBrandInsertRepository
    {
        public async Task<BrandEntity> AddBrandAsync(BrandEntity brand, CancellationToken cancellationToken)
        {
            context.Brands.Add(brand);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return brand;
        }
    }
}
