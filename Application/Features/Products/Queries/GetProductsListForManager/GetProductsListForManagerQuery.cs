using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Domain.Constants;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetProductsListForManager;

public sealed record GetProductsListForManagerQuery : IRequest<Result<PagedResult<ProductDetailForManagerResponse>>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public List<string> StatusIds { get; init; } = [];

    public string? Sorts { get; init; }

    public string? Filters { get; init; }

    public string? InventoryStatusFilter { get; init; }

    public SortDirection? SortByInventoryStatus { get; init; }

    public static GetProductsListForManagerQuery FromRequest(SieveModel request)
    {
        var search = ExtractFilterValue(request.Filters, "name");
        var statusIds = ExtractFilterValue(request.Filters, "statusIds")?.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries)
                .ToList() ??
            [];

        var inventoryStatusFilter = ExtractFilterValue(request.Filters, "inventoryStatus");

        var sortByInventoryStatus = request.Sorts is not null &&
                request.Sorts.Contains("inventoryStatus", StringComparison.OrdinalIgnoreCase)
            ? request.Sorts.TrimStart().StartsWith('-') ? SortDirection.Descending : SortDirection.Ascending
            : (SortDirection?)null;

        return new GetProductsListForManagerQuery
        {
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            Search = search,
            StatusIds = statusIds,
            Sorts = request.Sorts,
            Filters = request.Filters,
            InventoryStatusFilter = inventoryStatusFilter,
            SortByInventoryStatus = sortByInventoryStatus
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
            var keyValue = part.Split([ '=', '@', '!', '<', '>' ], 2);
            if(keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                var value = keyValue[1].Trim();
                return value.TrimStart('=', '@', '!', '<', '>', '*');
            }
        }

        return null;
    }
}
