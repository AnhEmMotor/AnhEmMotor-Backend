using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.Products.Queries.GetProductsList;

public sealed record GetProductsListQuery : IRequest<Result<PagedResult<ProductListStoreResponse>>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public List<string> StatusIds { get; init; } = [];

    public string? Sorts { get; init; }

    public string? Filters { get; init; }

    public List<int> CategoryIds { get; init; } = [];

    public List<int> OptionValueIds { get; init; } = [];

    public static GetProductsListQuery FromRequest(GetProductsRequest request)
    {
        var search = ExtractFilterValue(request.Filters, "search");
        var statusIds = ExtractFilterValue(request.Filters, "statusIds")?.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries)
                .ToList() ??
            [];

        var categoryIds = request.CategoryIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ??
            [];

        var optionValueIds = request.OptionValueIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ??
            [];

        var filters = request.Filters;
        if(!string.IsNullOrWhiteSpace(filters))
        {
            var filterParts = filters.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(
                    f =>
                    {
                        var trimmed = f.Trim();
                        return !trimmed.StartsWith("search", StringComparison.OrdinalIgnoreCase) &&
                            !trimmed.StartsWith("statusIds", StringComparison.OrdinalIgnoreCase);
                    })
                .ToList();
            filters = filterParts.Count > 0 ? string.Join(",", filterParts) : null;
        }

        return new GetProductsListQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            Search = search,
            StatusIds = statusIds,
            CategoryIds = categoryIds,
            OptionValueIds = optionValueIds,
            Sorts = request.Sorts,
            Filters = filters
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
                return keyValue[1].Trim().TrimStart('=', '@', '!', '<', '>');
            }
        }

        return null;
    }
}