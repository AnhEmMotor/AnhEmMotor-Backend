using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.Brands.Queries.GetBrandsList;

public sealed class GetBrandsListQueryHandler(IBrandSelectRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(GetBrandsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetBrands();
        ApplyDefaults(request.SieveModel);

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

    private static void ApplyDefaults(SieveModel sieveModel)
    {
        sieveModel.Page ??= 1;
        sieveModel.PageSize ??= 10;
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = $"-id";
        }
        else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
        {
            sieveModel.Sorts = $"{sieveModel.Sorts},-id";
        }
    }
}
