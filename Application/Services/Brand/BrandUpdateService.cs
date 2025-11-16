using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

namespace Application.Services.Brand
{
    public class BrandUpdateService(IBrandSelectRepository brandSelectRepository, IBrandUpdateRepository brandUpdateRepository) : IBrandUpdateService
    {
        public async Task<ErrorResponse?> UpdateBrandAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (brand == null)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Brand with Id {id} not found." }]
                };
            }
            if (request.Name is not null)
                brand.Name = request.Name;
            if (request.Description is not null)
                brand.Description = request.Description;
            await brandUpdateRepository.UpdateBrandAsync(brand, cancellationToken).ConfigureAwait(false);
            return null;
        }

        public async Task<ErrorResponse?> RestoreBrandAsync(int id, CancellationToken cancellationToken)
        {
            var brandList = await brandSelectRepository.GetDeletedBrandsByIdsAsync([id], cancellationToken).ConfigureAwait(false);
            if (brandList.Count == 0)
            {
                return new ErrorResponse
                {
                    Errors =
                    [
                        new ErrorDetail { Message = $"Deleted brand with Id {id} not found." }
                    ]
                };
            }
            await brandUpdateRepository.RestoreBrandAsync(brandList[0], cancellationToken).ConfigureAwait(false);
            return null;
        }


        public async Task<ErrorResponse?> RestoreBrandsAsync(RestoreManyBrandsRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();
            var deletedBrands = await brandSelectRepository.GetDeletedBrandsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);
            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(request.Ids, cancellationToken).ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var brand = allBrands.FirstOrDefault(b => b.Id == id);
                var deletedBrand = deletedBrands.FirstOrDefault(b => b.Id == id);

                if (brand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand not found",
                        Field = $"Brand ID: {id}"
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
                await brandUpdateRepository.RestoreBrandsAsync(deletedBrands, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}
