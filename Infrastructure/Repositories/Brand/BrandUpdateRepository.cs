using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandUpdateRepository(ApplicationDBContext context) : IBrandUpdateRepository
{
    public void Update(BrandEntity brand) { context.Brands.Update(brand); }

    public void Restore(BrandEntity brand) { context.Restore(brand); }

    public void Restore(IEnumerable<BrandEntity> brands)
    {
        foreach(var brand in brands)
        {
            context.Restore(brand);
        }
    }
}