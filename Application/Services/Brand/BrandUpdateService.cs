using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Helpers;

namespace Application.Services.Brand
{
    public class BrandUpdateService(IBrandSelectRepository brandSelectRepository, IBrandUpdateRepository brandUpdateRepository, IUnitOfWork unitOfWork) : IBrandUpdateService
    {
        public async Task<(BrandResponse? Data, ErrorResponse? Error)> UpdateBrandAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (brand == null)
            {
                return (
                    null,
                    new ErrorResponse
                    {
                        Errors = [new ErrorDetail { Field = "Id", Message = $"Brand with Id {id} not found." }]
                    }
                );
            }

            if (request.Name is not null)
                brand.Name = request.Name;

            if (request.Description is not null)
                brand.Description = request.Description;

            brandUpdateRepository.UpdateBrand(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var response = new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            };

            return (response, null);
        }

        public async Task<(BrandResponse? Data, ErrorResponse? Error)> RestoreBrandAsync(int id, CancellationToken cancellationToken)
        {
            var brandList = await brandSelectRepository.GetDeletedBrandsByIdsAsync([id], cancellationToken).ConfigureAwait(false);

            if (brandList.Count == 0)
            {
                return (
                    null,
                    new ErrorResponse
                    {
                        Errors = [new ErrorDetail { Field = "Id", Message = $"Deleted brand with Id {id} not found." }]
                    }
                );
            }

            var brand = brandList[0];
            brandUpdateRepository.RestoreBrand(brand);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var response = new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            };

            return (response, null);
        }

        public async Task<(List<int>? Data, ErrorResponse? Error)> RestoreBrandsAsync(RestoreManyBrandsRequest request, CancellationToken cancellationToken)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return ([], null);
            }

            var uniqueIds = request.Ids.Distinct().ToList();

            var deletedBrands = await brandSelectRepository.GetDeletedBrandsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);
            var allBrands = await brandSelectRepository.GetAllBrandsByIdsAsync(uniqueIds, cancellationToken).ConfigureAwait(false);

            var allBrandsMap = allBrands.ToDictionary(b => b.Id!);
            var deletedBrandsSet = deletedBrands.Select(b => b.Id!).ToHashSet();

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

                if (!deletedBrandsSet.Contains(id))
                {
                    var brandName = allBrandsMap[id].Name;
                    errorDetails.Add(new ErrorDetail
                    {
                        Field = "Id",
                        Message = $"Brand '{brandName}' (Id: {id}) is already active"
                    });
                }
            }

            if (errorDetails.Count > 0)
            {
                return (null, new ErrorResponse { Errors = errorDetails });
            }

            if (deletedBrands.Count > 0)
            {
                brandUpdateRepository.RestoreBrands(deletedBrands);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return (uniqueIds, null);
        }
    }
}
