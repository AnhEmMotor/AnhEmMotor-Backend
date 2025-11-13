using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

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

        public async Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var activeBrands = await brandSelectRepository.GetActiveBrandsByIdsAsync(request.Ids);
            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(request.Ids);

            foreach (var id in request.Ids)
            {
                var brand = allBrands.FirstOrDefault(b => b.Id == id);
                var activeBrand = activeBrands.FirstOrDefault(b => b.Id == id);

                if (brand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand not found",
                        Field = "Brand ID: " + id.ToString()
                    });
                }
                else if (activeBrand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand has already been deleted",
                        Field = brand.Name
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (activeBrands.Count > 0)
            {
                await brandDeleteRepository.DeleteBrandsAsync(activeBrands);
            }

            return null;
        }
    }
}
