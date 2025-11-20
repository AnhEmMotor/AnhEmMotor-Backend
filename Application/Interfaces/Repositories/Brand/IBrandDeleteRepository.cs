using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandDeleteRepository
    {
        void DeleteBrand(BrandEntity brand);
        void DeleteBrands(List<BrandEntity> brands);
    }
}
