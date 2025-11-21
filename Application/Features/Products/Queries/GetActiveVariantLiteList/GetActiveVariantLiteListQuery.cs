using Application.ApiContracts.Product.Common;
using Domain.Helpers;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetActiveVariantLiteList;

public sealed record GetActiveVariantLiteListQuery : IRequest<PagedResult<ProductVariantLiteResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public List<string> StatusIds { get; init; } = [];
    public string? Sorts { get; init; }

    public static GetActiveVariantLiteListQuery FromRequest(SieveModel request)
    {
        var search = ExtractFilterValue(request.Filters, "search");
        var statusIds = ExtractFilterValue(request.Filters, "statusIds")?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? [];

        return new GetActiveVariantLiteListQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            Search = search,
            StatusIds = statusIds,
            Sorts = request.Sorts
        };
    }

    private static string? ExtractFilterValue(string? filters, string key)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }

        var parts = filters.Split(',');
        foreach (var part in parts)
        {
            var keyValue = part.Split(['=', '@', '!'], 2);
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return keyValue[1].Trim();
            }
        }

        return null;
    }
}
