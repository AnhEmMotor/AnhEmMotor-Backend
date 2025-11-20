using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

namespace Application.Services.Brand
{
    public class BrandDeleteService(IBrandSelectRepository brandSelectRepository, IBrandDeleteRepository brandDeleteRepository, IUnitOfWork unitOfWork) : IBrandDeleteService
    {
        public async Task<ErrorResponse?> DeleteBrandAsync(int id, CancellationToken cancellationToken)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (brand == null)
            {
                return new ErrorResponse
                {
                    Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {id} not found." }]
                };
            }

            brandDeleteRepository.DeleteBrand(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return null;
        }

        public async Task<ErrorResponse?> DeleteBrandsAsync(DeleteManyBrandsRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return null;
            }

            var uniqueIds = request.Ids.Distinct().ToList();

            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
            var activeBrands = await brandSelectRepository.GetActiveBrandsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

            var allBrandsMap = allBrands.ToDictionary(b => b.Id!);
            var activeBrandsSet = activeBrands.Select(b => b.Id!).ToHashSet();

            var errorDetails = new List<ErrorDetail>();

            foreach (var id in uniqueIds)
            {
                if (!allBrandsMap.ContainsKey(id))
                {
                    errorDetails.Add(new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Brand with Id {id} not found"
                    });
                    continue;
                }

                if (!activeBrandsSet.Contains(id))
                {
                    var brandName = allBrandsMap[id].Name;
                    errorDetails.Add(new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Brand '{brandName}' (Id: {id}) has already been deleted"
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return new ErrorResponse { Errors = errorDetails };
            }

            if (activeBrands.Count > 0)
            {
                brandDeleteRepository.DeleteBrands(activeBrands);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return null;
        }
    }
}