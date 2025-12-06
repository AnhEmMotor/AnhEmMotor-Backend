using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Shared;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandReadRepository repository, IPaginator paginator) : IRequestHandler<GetBrandsListQuery, PagedResult<ApiContracts.Brand.Responses.BrandResponse>>
{
    public Task<PagedResult<ApiContracts.Brand.Responses.BrandResponse>> Handle(
        GetBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<BrandEntity, ApiContracts.Brand.Responses.BrandResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}