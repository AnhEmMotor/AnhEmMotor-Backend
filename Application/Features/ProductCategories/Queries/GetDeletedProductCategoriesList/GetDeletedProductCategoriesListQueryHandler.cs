using Application.ApiContracts.Brand;
using Application.Features.Brands.Queries.GetDeletedBrandsList;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Domain.Shared;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.ProductCategories.Queries.GetDeletedProductCategoriesList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, ICustomSievePaginator paginator) : IRequestHandler<GetDeletedBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(
        GetDeletedBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return await paginator.ApplyAsync<BrandEntity, BrandResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}
