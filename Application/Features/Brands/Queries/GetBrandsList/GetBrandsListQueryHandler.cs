using Application.ApiContracts.Brand.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using MediatR;
using BrandEntity = Domain.Entities.Brand;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetBrandsListQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(
        GetBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return await paginator.ApplyAsync<BrandEntity, BrandResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken);
    }
}