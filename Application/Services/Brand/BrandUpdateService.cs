using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;

namespace Application.Services.Brand
{
    public class BrandUpdateService(IBrandSelectRepository brandSelectRepository, IBrandUpdateRepository brandUpdateRepository) : IBrandUpdateService
    {
        public async Task<bool> UpdateBrandAsync(int id, UpdateBrandRequest request)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id);

            if (brand == null) return false;

            brand.Name = request.Name;
            brand.Description = request.Description;

            await brandUpdateRepository.UpdateBrandAsync(brand);
            return true;
        }

        public async Task<bool> RestoreBrandAsync(int id)
        {
            var brandList = await brandSelectRepository.GetDeletedBrandsByIdsAsync([id]);

            if (brandList.Count == 0) return false;

            await brandUpdateRepository.RestoreBrandAsync(brandList[0]);
            return true;
        }

        public async Task<bool> RestoreBrandsAsync(RestoreManyBrandsRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0) return false;

            var brands = await brandSelectRepository.GetDeletedBrandsByIdsAsync(request.Ids);

            if (brands.Count == 0) return false;

            await brandUpdateRepository.RestoreBrandsAsync(brands);
            return true;
        }
    }
}
