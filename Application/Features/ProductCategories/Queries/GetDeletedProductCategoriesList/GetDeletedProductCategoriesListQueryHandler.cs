using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetDeletedProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetDeletedProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<ProductCategoryEntity, ProductCategoryResponse>(
            query,
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken).ConfigureAwait(false);
    }
}
