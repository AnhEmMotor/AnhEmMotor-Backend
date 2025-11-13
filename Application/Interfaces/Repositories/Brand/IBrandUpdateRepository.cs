using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandUpdateRepository
    {
        Task UpdateBrandAsync(BrandEntity brand);
        Task RestoreBrandAsync(BrandEntity brand);
        Task RestoreBrandsAsync(List<BrandEntity> brands);
    }
}
