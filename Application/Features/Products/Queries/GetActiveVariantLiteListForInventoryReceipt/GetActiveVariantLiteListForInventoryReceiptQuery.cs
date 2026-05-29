using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Products.Queries.GetActiveVariantLiteListForInventoryReceipt
{
    public class GetActiveVariantLiteListForInventoryReceiptQuery : IRequest<Result<PagedResult<ProductVariantLiteResponseForInventoryReceipt>>>
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 10;

        public string? Search { get; init; }

        public List<string> StatusIds { get; init; } = [];

        public string? Sorts { get; init; }

        public string? Filters { get; init; }

        public static GetActiveVariantLiteListForInventoryReceiptQuery FromRequest(SieveModel request)
        {
            var search = ExtractFilterValue(request.Filters, "search");
            var displayName = ExtractFilterValue(request.Filters, "DisplayName");
            if (string.IsNullOrWhiteSpace(search) && !string.IsNullOrWhiteSpace(displayName))
            {
                search = displayName;
            }

            var statusIds = ExtractFilterValue(request.Filters, "statusIds")?.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ??
                [];

            var cleanFilters = RemoveFilter(request.Filters, "DisplayName");

            return new GetActiveVariantLiteListForInventoryReceiptQuery
            {
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 10,
                Search = search,
                StatusIds = statusIds,
                Sorts = request.Sorts,
                Filters = cleanFilters
            };
        }

        private static string? RemoveFilter(string? filters, string key)
        {
            if (string.IsNullOrWhiteSpace(filters))
            {
                return null;
            }
            var parts = filters.Split(',');
            var remainingParts = new List<string>();
            foreach (var part in parts)
            {
                var keyValue = part.Split(['=', '@', '!'], 2);
                if (keyValue.Length == 2 && keyValue[0].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                remainingParts.Add(part);
            }
            return remainingParts.Count > 0 ? string.Join(",", remainingParts) : null;
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
                    var value = keyValue[1].Trim();
                    return value.TrimStart('=', '@', '!', '<', '>', '_');
                }
            }
            return null;
        }
    }
}
