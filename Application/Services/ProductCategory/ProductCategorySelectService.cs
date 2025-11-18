using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Services.ProductCategory;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Linq;

namespace Application.Services.ProductCategory
{
    public class ProductCategorySelectService(IProductCategorySelectRepository selectRepository, ISieveProcessor sieveProcessor) : IProductCategorySelectService
    {
        public async Task<(ProductCategoryResponse? Data, ErrorResponse? Error)> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var category = await selectRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return (
                    null,
                    new ErrorResponse
                    {
                        Errors =
                        [
                            new ErrorDetail { Message = $"Product category with Id {id} not found." }
                        ]
                    }
                );
            }

            return (
                new ProductCategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                },
                null
            );
        }

        public async Task<PagedResult<ProductCategoryResponse>> GetProductCategoriesAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = selectRepository.GetProductCategories();
            ApplyDefaults(sieveModel);
            var filteredQuery = sieveProcessor.Apply(sieveModel, query);
            var categories = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

            var responses = categories.Select(category => new ProductCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            }).ToList();

            return new PagedResult<ProductCategoryResponse>(
                responses,
                totalCount,
                sieveModel.Page!.Value,
                sieveModel.PageSize!.Value
            );
        }

        public async Task<PagedResult<ProductCategoryResponse>> GetDeletedProductCategoriesAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = selectRepository.GetDeletedProductCategories();
            ApplyDefaults(sieveModel);
            var filteredQuery = sieveProcessor.Apply(sieveModel, query);
            var categories = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

            var responses = categories.Select(category => new ProductCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            }).ToList();

            return new PagedResult<ProductCategoryResponse>(
                responses,
                totalCount,
                sieveModel.Page!.Value,
                sieveModel.PageSize!.Value
            );
        }

        private static void ApplyDefaults(SieveModel sieveModel)
        {
            sieveModel.Page ??= 1;
            sieveModel.PageSize ??= int.MaxValue;

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
