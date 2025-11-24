using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Sieve;
using Domain.Enums;
using Domain.Shared;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandReadRepository repository, ISieveProcessor sieveProcessor): IRequestHandler<GetDeletedBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(GetDeletedBrandsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable(DataFetchMode.DeletedOnly);
        SieveHelper.ApplyDefaultSorting(request.SieveModel, DataFetchMode.DeletedOnly);

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var brands = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<BrandResponse>(brands.Adapt<List<BrandResponse>>(), totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
