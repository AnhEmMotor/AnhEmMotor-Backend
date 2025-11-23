using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandDeleteRepository(ApplicationDBContext context) : IBrandDeleteRepository
{
    public void Delete(BrandEntity brand)
    {
        context.Brands.Remove(brand);
    }

    public void Delete(IEnumerable<BrandEntity> brands)
    {
        context.Brands.RemoveRange(brands);
    }
}