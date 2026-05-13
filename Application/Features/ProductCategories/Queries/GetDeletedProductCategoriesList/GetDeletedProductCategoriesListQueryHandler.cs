using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetDeletedProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetDeletedProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<ProductCategoryResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
