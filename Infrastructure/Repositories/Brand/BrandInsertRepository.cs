using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandInsertRepository(ApplicationDBContext context) : IBrandInsertRepository
{
    public async Task AddAsync(BrandEntity brand, CancellationToken cancellationToken)
    {
        await context.Brands.AddAsync(brand, cancellationToken).ConfigureAwait(false);
    }
}