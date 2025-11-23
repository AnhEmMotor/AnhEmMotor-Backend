using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(IProductCategorySelectRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedProductCategoriesListQuery, PagedResult<ProductCategoryResponse>>
{
    public async Task<PagedResult<ProductCategoryResponse>> Handle(GetDeletedProductCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetDeletedProductCategories();
        ApplyDefaults(request.SieveModel);

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var categories = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        var responses = categories.Select(c => new ProductCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        return new PagedResult<ProductCategoryResponse>(responses, totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }

    private static void ApplyDefaults(SieveModel sieveModel)
    {
        sieveModel.Page ??= 1;
        sieveModel.PageSize ??= int.MaxValue;
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = $"-{AuditingProperties.CreatedAt}";
        }
        else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
        {
            sieveModel.Sorts = $"{sieveModel.Sorts},-{AuditingProperties.CreatedAt}";
        }
    }
}
