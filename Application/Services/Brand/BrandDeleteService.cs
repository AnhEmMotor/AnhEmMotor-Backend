using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;

namespace Application.Services.Brand
{
    public class BrandDeleteService(IBrandSelectRepository brandSelectRepository, IBrandDeleteRepository brandDeleteRepository) : IBrandDeleteService
    {
        public async Task<bool> DeleteBrandAsync(int id)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id);

            if (brand == null) return false;

            await brandDeleteRepository.DeleteBrandAsync(brand);
            return true;
        }

        public async Task<bool> DeleteBrandsAsync(DeleteManyBrandsRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0) return false;

            var brands = await brandSelectRepository.GetActiveBrandsByIdsAsync(request.Ids);

            if (brands.Count == 0) return false;

            await brandDeleteRepository.DeleteBrandsAsync(brands);
            return true;
        }
    }
}
