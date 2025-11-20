using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Services.Brand;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Services.Brand
{
    public class BrandSelectService(IBrandSelectRepository brandSelectRepository, ISieveProcessor sieveProcessor) : IBrandSelectService
    {
        public async Task<(BrandResponse? Data, ErrorResponse? Error)> GetBrandByIdAsync(int id, CancellationToken cancellationToken)
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

            var response = new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            };

            return (response, null);
        }

        public async Task<PagedResult<BrandResponse>> GetBrandsAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = brandSelectRepository.GetBrands();

            ApplyDefaultsToSieveModel(sieveModel);

            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                return new PagedResult<BrandResponse>([], 0, 1, 1);
            }

            var brandsQuery = sieveProcessor.Apply(sieveModel, query);
            var brands = await brandsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

            var brandResponses = brands.Select(brand => new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            }).ToList();

            return new PagedResult<BrandResponse>(
                brandResponses,
                totalCount,
                sieveModel.Page.Value,
                sieveModel.PageSize.Value
            );
        }

        public async Task<PagedResult<BrandResponse>> GetDeletedBrandsAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = brandSelectRepository.GetDeletedBrands();

            ApplyDefaultsToSieveModel(sieveModel);

            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                return new PagedResult<BrandResponse>([], 0, 1, 1);
            }

            var brandsQuery = sieveProcessor.Apply(sieveModel, query);
            var brands = await brandsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

            var brandResponses = brands.Select(brand => new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            }).ToList();

            return new PagedResult<BrandResponse>(
                brandResponses,
                totalCount,
                sieveModel.Page.Value,
                sieveModel.PageSize.Value
            );
        }

        private static void ApplyDefaultsToSieveModel(SieveModel sieveModel)
        {
            sieveModel.Page ??= 1;
            sieveModel.PageSize ??= 10;

            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-id";
            }
            else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
            {
                sieveModel.Sorts = $"{sieveModel.Sorts},-id";
            }
        }
    }
}
