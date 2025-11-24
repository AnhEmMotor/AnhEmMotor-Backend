using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Sieve;
using Domain.Enums;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(IProductCategoryReadRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedProductCategoriesListQuery, PagedResult<ProductCategoryResponse>>
{
    public async Task<PagedResult<ProductCategoryResponse>> Handle(GetDeletedProductCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);
        SieveHelper.ApplyDefaultSorting(request.SieveModel, isApplyDefaultPageAndPageSize: false);

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var categories = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<ProductCategoryResponse>(categories.Adapt<List<ProductCategoryResponse>>(), totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
