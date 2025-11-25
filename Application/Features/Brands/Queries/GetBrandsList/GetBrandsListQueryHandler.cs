using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Shared;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandReadRepository repository, ICustomSievePaginator paginator) : IRequestHandler<GetBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(
        GetBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return await paginator.ApplyAsync<BrandEntity, BrandResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}