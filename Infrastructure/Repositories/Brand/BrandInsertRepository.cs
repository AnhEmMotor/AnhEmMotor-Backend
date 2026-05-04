using Application.Interfaces.Repositories.Brand;
using Infrastructure.DBContexts;
using BrandEntity = Domain.Entities.Brand;

namespace Infrastructure.Repositories.Brand;

public class BrandInsertRepository(ApplicationDBContext context) : IBrandInsertRepository
{
    public void Add(BrandEntity brand)
    {
        context.Brands.Add(brand);
    }
}