using Application.ApiContracts.Brand;
using Application.Interfaces.Repositories.Brand;
using Domain.Enums;
using Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Features.Brands.Queries.GetDeletedBrandsList;

public sealed class GetDeletedBrandsListQueryHandler(IBrandSelectRepository repository, ISieveProcessor sieveProcessor)
    : IRequestHandler<GetDeletedBrandsListQuery, PagedResult<BrandResponse>>
{
    public async Task<PagedResult<BrandResponse>> Handle(GetDeletedBrandsListQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetDeletedBrands();
        ApplyDefaults(request.SieveModel);

        if (request.SieveModel.Page == null || request.SieveModel.PageSize == null)
        {
            return new PagedResult<BrandResponse>([], 0, 1, 1);
        }

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);
        var brands = await filteredQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = await sieveProcessor.Apply(request.SieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

        var responses = brands.Select(b => new BrandResponse
        {
            Id = b.Id,
            Name = b.Name,
            Description = b.Description
        }).ToList();

        return new PagedResult<BrandResponse>(responses, totalCount, request.SieveModel.Page.Value, request.SieveModel.PageSize.Value);
    }

    private static void ApplyDefaults(SieveModel sieveModel)
    {
        sieveModel.Page ??= 1;
        sieveModel.PageSize ??= int.MaxValue;
        if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
        {
            sieveModel.Sorts = $"-{AuditingProperties.CreatedAt}";
        }
        else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
        {
            sieveModel.Sorts = $"{sieveModel.Sorts},-{AuditingProperties.CreatedAt}";
        }
    }
}
