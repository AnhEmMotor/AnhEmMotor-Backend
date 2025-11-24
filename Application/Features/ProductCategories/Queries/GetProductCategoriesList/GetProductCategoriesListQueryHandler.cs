using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Sieve;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(IProductCategoryReadRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetProductCategoriesListQuery, PagedResult<ProductCategoryResponse>>
{
    public async Task<PagedResult<ProductCategoryResponse>> Handle(GetProductCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        SieveHelper.ApplyDefaultSorting(request.SieveModel, isApplyDefaultPageAndPageSize: false);
        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var categories = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<ProductCategoryResponse>(categories.Adapt<List<ProductCategoryResponse>>(), totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
