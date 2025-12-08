using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetProductCategoriesList;

public sealed class GetProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetProductCategoriesListQuery, Domain.Primitives.PagedResult<ProductCategoryResponse>>
{
    public Task<Domain.Primitives.PagedResult<ProductCategoryResponse>> Handle(
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