using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandsList;

public class GetBrandsListQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetBrandsListQuery, Result<PagedResult<BrandResponse>>>
{
    public async Task<Result<PagedResult<BrandResponse>>> Handle(
        GetBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<BrandResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
