using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

namespace Application.Services.Brand
{
    public class BrandDeleteService(IBrandSelectRepository brandSelectRepository, IBrandDeleteRepository brandDeleteRepository) : IBrandDeleteService
    {
        public async Task<ErrorResponse?> DeleteBrandAsync(int id, CancellationToken cancellationToken)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (brand == null)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Message = $"Brand with Id {id} not found." }]
                };
            }

            await brandDeleteRepository.DeleteBrandAsync(brand, cancellationToken).ConfigureAwait(false);

            return null;
        }

        public async Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var errorDetails = new List<ErrorDetail>();

            var activeBrands = await brandSelectRepository.GetActiveBrandsByIdsAsync(request.Ids, cancellationToken)
                .ConfigureAwait(false);

            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(request.Ids, cancellationToken)
                .ConfigureAwait(false);

            foreach (var id in request.Ids)
            {
                var brand = allBrands.FirstOrDefault(b => b.Id == id);
                var activeBrand = activeBrands.FirstOrDefault(b => b.Id == id);

                if (brand == null)
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Message = "Brand not found",
                        Field = $"Brand ID: {id}"
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
                await brandDeleteRepository.DeleteBrandsAsync(activeBrands, cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}