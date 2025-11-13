using Application.ApiContracts.Brand;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Interfaces.Repositories.Brand
{
    public interface IBrandDeleteRepository
    {
        Task DeleteBrandAsync(BrandEntity brand);
        Task DeleteBrandsAsync(List<BrandEntity> brands);
    }
}
