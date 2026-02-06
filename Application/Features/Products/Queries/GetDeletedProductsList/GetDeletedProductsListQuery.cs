using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetDeletedProductsList;

public sealed record GetDeletedProductsListQuery : IRequest<Result<PagedResult<ProductDetailForManagerResponse>>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public List<string> StatusIds { get; init; } = [];

    public string? Sorts { get; init; }

    public string? Filters { get; init; }

    public static GetDeletedProductsListQuery FromRequest(SieveModel request)
    {
        var search = ExtractFilterValue(request.Filters, "search");
        var statusIds = ExtractFilterValue(request.Filters, "statusIds")?.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries)
                .ToList() ??
            [];

        return new GetDeletedProductsListQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            Search = search,
            StatusIds = statusIds,
            Sorts = request.Sorts,
            Filters = request.Filters
        };
    }

    private static string? ExtractFilterValue(string? filters, string key)
    {
        if(string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }

        var parts = filters.Split(',');
        foreach(var part in parts)
        {
            var keyValue = part.Split([ '=', '@', '!' ], 2);
            if(keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return keyValue[1].Trim();
            }
        }

        return null;
    }
}
