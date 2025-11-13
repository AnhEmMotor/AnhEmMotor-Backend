using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

namespace Application.Services.Brand
{
    public class BrandUpdateService(IBrandSelectRepository brandSelectRepository, IBrandUpdateRepository brandUpdateRepository) : IBrandUpdateService
    {
        public async Task<bool> UpdateBrandAsync(int id, UpdateBrandRequest request)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id);

            if (brand == null) return false;

            if (request.Name is not null)
                brand.Name = request.Name;
            if (request.Description is not null)
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

        public async Task<ErrorResponse?> RestoreBrandsAsync(RestoreManyBrandsRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var deletedBrands = await brandSelectRepository.GetDeletedBrandsByIdsAsync(request.Ids);
            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(request.Ids);

            foreach (var id in request.Ids)
            {
                var brand = allBrands.FirstOrDefault(b => b.Id == id);
                var deletedBrand = deletedBrands.FirstOrDefault(b => b.Id == id);

                if (brand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand not found",
                        Field = "Brand ID: " + id.ToString()
                    });
                }
                else if (deletedBrand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand has already been restored",
                        Field = brand.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (deletedBrands.Count > 0)
            {
                await brandUpdateRepository.RestoreBrandsAsync(deletedBrands);
            }

            return null;
        }
    }
}
