using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandInsertRepository
    {
        Task<BrandEntity> AddBrandAsync(BrandEntity brand);
    }
}
