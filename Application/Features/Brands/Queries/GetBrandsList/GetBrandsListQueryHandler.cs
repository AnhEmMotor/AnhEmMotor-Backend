using Application.ApiContracts.Brand.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandReadRepository repository, IPaginator paginator) : IRequestHandler<GetBrandsListQuery, Domain.Primitives.PagedResult<BrandResponse>>
{
    public Task<Domain.Primitives.PagedResult<BrandResponse>> Handle(
        GetBrandsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();

        return paginator.ApplyAsync<BrandEntity, BrandResponse>(
            query,
            request.SieveModel,
            cancellationToken: cancellationToken);
    }
}