using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Primitives;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetProductCategoriesListQuery, Result<PagedResult<ProductCategoryResponse>>>
{
    public async Task<Result<PagedResult<ProductCategoryResponse>>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return await paginator.ApplyAsync<ProductCategoryEntity, ProductCategoryResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}