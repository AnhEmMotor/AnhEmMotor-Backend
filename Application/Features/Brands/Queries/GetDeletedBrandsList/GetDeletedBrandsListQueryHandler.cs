using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetDeletedBrandsListQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(
        GetDeletedBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<BrandResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}