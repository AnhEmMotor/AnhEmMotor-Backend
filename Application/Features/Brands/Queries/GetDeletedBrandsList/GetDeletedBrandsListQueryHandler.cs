using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository) : IRequestHandler<GetDeletedBrandsListQuery, Result<PagedResult<BrandRestoreResponse>>>
{
    public async Task<Result<PagedResult<BrandRestoreResponse>>> Handle(
        GetDeletedBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<BrandRestoreResponse>(
            request.SieveModel!,
            DataFetchMode.DeletedOnly,
            cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
