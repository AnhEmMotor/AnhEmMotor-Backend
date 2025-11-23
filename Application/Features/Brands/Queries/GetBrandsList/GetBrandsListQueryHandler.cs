using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Application.Sieve;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandReadRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(GetBrandsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        SieveHelper.ApplyDefaultSorting(request.SieveModel);

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var brands = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        var responses = brands.Select(b => new BrandResponse
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description
        }).ToList();

        return new PagedResult<BrandResponse>(responses, totalCount, request.SieveModel.Page!.Value, request.SieveModel.PageSize!.Value);
    }
}
