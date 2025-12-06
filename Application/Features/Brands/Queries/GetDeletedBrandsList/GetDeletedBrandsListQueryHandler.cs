using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Shared;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, IPaginator paginator) : IRequestHandler<GetDeletedBrandsListQuery, PagedResult<ApiContracts.Brand.Responses.BrandResponse>>
{
    public Task<PagedResult<ApiContracts.Brand.Responses.BrandResponse>> Handle(
        GetDeletedBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        return paginator.ApplyAsync<BrandEntity, ApiContracts.Brand.Responses.BrandResponse>(
            query,
            request.SieveModel,
            DataFetchMode.DeletedOnly,
            cancellationToken);
    }
}