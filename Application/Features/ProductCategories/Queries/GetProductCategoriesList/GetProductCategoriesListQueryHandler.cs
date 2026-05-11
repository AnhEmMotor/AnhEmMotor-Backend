using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(IProductCategoryReadRepository repository) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<ProductCategoryResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}