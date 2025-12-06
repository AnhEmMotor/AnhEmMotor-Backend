using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Shared;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    IPaginator paginator) : IRequestHandler<GetProductCategoriesListQuery, PagedResult<ProductCategoryResponse>>
{
    public Task<PagedResult<ProductCategoryResponse>> Handle(
        GetProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<ProductCategoryEntity, ProductCategoryResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}