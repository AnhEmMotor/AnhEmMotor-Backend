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

    public List<int> BrandIds { get; init; } = [];

    public List<int> OptionValueIds { get; init; } = [];

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public static GetProductsListQuery FromRequest(
        GetProductsRequest request,
        decimal? minPriceParam = null,
        decimal? maxPriceParam = null)
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
        var brandIds = request.BrandIds?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList() ??
            [];
        var optionValueIds = request.OptionValueIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ??
            [];
        var minPrice = minPriceParam ??
            request.MinPrice ??
            (decimal.TryParse(ExtractFilterValue(request.Filters, "price", ">="), out var min) ? min : (decimal?)null);
        var maxPrice = maxPriceParam ??
            request.MaxPrice ??
            (decimal.TryParse(ExtractFilterValue(request.Filters, "price", "<="), out var max) ? max : (decimal?)null);
        var filters = request.Filters;
        if (!string.IsNullOrWhiteSpace(filters))
        {
            var filterParts = filters.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(
                    f =>
                    {
                        var trimmed = f.Trim();
                        return !trimmed.StartsWith("search", StringComparison.OrdinalIgnoreCase) &&
                            !trimmed.StartsWith("statusIds", StringComparison.OrdinalIgnoreCase) &&
                            !trimmed.StartsWith("price", StringComparison.OrdinalIgnoreCase);
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
            BrandIds = brandIds,
            OptionValueIds = optionValueIds,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Sorts = request.Sorts,
            Filters = filters
        };
    }

    private static string? ExtractFilterValue(string? filters, string key, string op = "")
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return null;
        }
        var parts = filters.Split(',');
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (!trimmedPart.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                continue;
            var operators = new[] { "<=", ">=", "<", ">", "==", "!", "@", "=" };
            var foundOp = operators.FirstOrDefault(o => trimmedPart[key.Length..].Trim().StartsWith(o));
            if (foundOp != null)
            {
                if (!string.IsNullOrEmpty(op) && string.Compare(foundOp, op) != 0)
                    continue;
                var value = trimmedPart[key.Length..].Trim()[foundOp.Length..].Trim();
                return value;
            }
        }
        return null;
    }
}