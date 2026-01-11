using Application.ApiContracts.Brand.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using MediatR;
using BrandEntity = Domain.Entities.Brand;
using Domain.Primitives;
using Application.Common.Models;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedBrandsListQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(
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