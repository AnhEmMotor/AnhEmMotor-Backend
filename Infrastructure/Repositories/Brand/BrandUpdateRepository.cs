using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandUpdateRepository(ApplicationDBContext context) : IBrandUpdateRepository
{
    public void UpdateBrand(BrandEntity brand)
    {
        context.Brands.Update(brand);
    }

    public void RestoreBrand(BrandEntity brand)
    {
        context.Restore(brand);
    }

    public void RestoreBrands(List<BrandEntity> brands)
    {
        foreach (var brand in brands)
        {
            context.Restore(brand);
        }
    }
}