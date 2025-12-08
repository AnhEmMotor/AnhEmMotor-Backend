using Application.ApiContracts.Brand.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetDeletedBrandsListQuery, Domain.Primitives.PagedResult<BrandResponse>>
{
    public Task<Domain.Primitives.PagedResult<BrandResponse>> Handle(
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