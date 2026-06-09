using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Queries.GetNewsBySlug;

public sealed class GetNewsBySlugQueryHandler(INewsReadRepository repository, IMemoryCache cache) : IRequestHandler<GetNewsBySlugQuery, Result<NewsForStoreResponse>>
{
    public async Task<Result<NewsForStoreResponse>> Handle(GetNewsBySlugQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"News_Slug_{request.Slug}_Store";
        if (cache.TryGetValue(cacheKey, out NewsForStoreResponse? cachedResponse))
        {
            return Result<NewsForStoreResponse>.Success(cachedResponse!);
        }

        var news = await repository.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (news == null)
        {
            return Result<NewsForStoreResponse>.Failure(new Error("News.NotFound", "The requested news was not found."));
        }

        var response = new NewsForStoreResponse
        {
            Id = news.Id,
            Title = news.Title,
            Content = news.Content ?? string.Empty,
            CategoryName = news.Category?.Name ?? "Tin tức",
            CoverImageUrl = news.CoverImageUrl ?? string.Empty,
            PublishedDate = news.PublishedDate,
            Excerpt = news.MetaDescription,
            LinkedProducts = news.LinkedProducts?.Select(lp => new NewsProductForStoreResponse
            {
                UrlSlug = lp.ProductVariant?.UrlSlug ?? string.Empty,
                VariantName = (lp.ProductVariant?.Product?.Name != null ? lp.ProductVariant.Product.Name + " - " : "") + (lp.ProductVariant?.VariantName ?? string.Empty),
                ColorName = lp.ProductVariantColor?.ColorName,
                ImageUrl = lp.ProductVariantColor?.CoverImageUrl ?? lp.ProductVariant?.CoverImageUrl ?? string.Empty
            }).ToList() ?? new List<NewsProductForStoreResponse>()
        };

        cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        return Result<NewsForStoreResponse>.Success(response);
    }
}
