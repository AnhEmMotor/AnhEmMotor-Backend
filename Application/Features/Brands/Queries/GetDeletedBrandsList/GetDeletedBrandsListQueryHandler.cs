using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedBrandsListQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(
        GetDeletedBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);

        var result = await paginator.ApplyAsync<BrandEntity, BrandResponse>(
            query,
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken).ConfigureAwait(false);

        return result;
    }
}