using System;

namespace Application.ApiContracts.News.Responses
{
    public record NewsProductForStoreResponse
    {
        public string UrlSlug { get; init; } = default!;

        public string VariantName { get; init; } = default!;

        public string? ColorName { get; init; }

        public string ImageUrl { get; init; } = default!;
    }
}
