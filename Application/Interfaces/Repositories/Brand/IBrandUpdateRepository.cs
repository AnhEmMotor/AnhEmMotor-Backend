using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandUpdateRepository
    {
        void UpdateBrand(BrandEntity brand);
        void RestoreBrand(BrandEntity brand);
        void RestoreBrands(List<BrandEntity> brands);
    }
}
