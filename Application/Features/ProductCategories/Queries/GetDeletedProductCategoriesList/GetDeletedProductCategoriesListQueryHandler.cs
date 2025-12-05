using Application.ApiContracts.ProductCategory;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Shared;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedProductCategoriesListQueryHandler(
    IProductCategoryReadRepository repository,
    IPaginator paginator) : IRequestHandler<GetDeletedProductCategoriesListQuery, PagedResult<ProductCategoryResponse>>
{
    public Task<PagedResult<ProductCategoryResponse>> Handle(
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
