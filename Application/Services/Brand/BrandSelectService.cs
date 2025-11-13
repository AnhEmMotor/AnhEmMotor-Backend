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
        public async Task<BrandResponse?> GetBrandByIdAsync(int id)
        {
            var brand = await brandSelectRepository.GetBrandByIdAsync(id);
            if (brand == null) return null;
            return new BrandResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description
            };
        }

        public async Task<PagedResult<BrandResponse>> GetBrandsAsync(SieveModel sieveModel)
        {
            var query = brandSelectRepository.GetBrands();
            ApplyDefaultsToSieveModel(sieveModel);
            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                int pageNumber = sieveModel.Page ?? 1;
                int pageSize = sieveModel.PageSize ?? 1;
                return new PagedResult<BrandResponse>(
                    [],
                    0,
                    pageNumber,
                    pageSize
                );
            }
            var brandsQuery = sieveProcessor.Apply(sieveModel, query);
            var brands = await brandsQuery.ToListAsync();
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync();
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

        public async Task<PagedResult<BrandResponse>> GetDeletedBrandsAsync(SieveModel sieveModel)
        {
            var query = brandSelectRepository.GetDeletedBrands();
            ApplyDefaultsToSieveModel(sieveModel);
            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                int pageNumber = sieveModel.Page ?? 1;
                int pageSize = sieveModel.PageSize ?? 1;
                return new PagedResult<BrandResponse>(
                    [],
                    0,
                    pageNumber,
                    pageSize
                );
            }
            var brandsQuery = sieveProcessor.Apply(sieveModel, query);
            var brands = await brandsQuery.ToListAsync();
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync();
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
