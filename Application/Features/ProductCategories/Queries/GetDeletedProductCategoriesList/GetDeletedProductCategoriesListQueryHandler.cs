using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    ISievePaginator paginator) : IRequestHandler<GetDeletedProductCategoriesListQuery, Domain.Primitives.PagedResult<ProductCategoryResponse>>
{
    public Task<Domain.Primitives.PagedResult<ProductCategoryResponse>> Handle(
        GetDeletedProductCategoriesListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return paginator.ApplyAsync<ProductCategoryEntity, ProductCategoryResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}
