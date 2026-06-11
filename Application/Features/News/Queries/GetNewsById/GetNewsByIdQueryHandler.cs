using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Queries.GetNewsById;

public sealed class GetNewsByIdQueryHandler(INewsReadRepository repository, IMemoryCache cache) : IRequestHandler<GetNewsByIdQuery, Result<NewsResponse>>
{
    public async Task<Result<NewsResponse>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"News_{request.Id}";
        if (cache.TryGetValue(cacheKey, out NewsResponse? cachedResponse))
        {
            return Result<NewsResponse>.Success(cachedResponse!);
        }
        var news = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (news == null)
        {
            return Result<NewsResponse>.Failure(new Error("News.NotFound", "The requested news was not found."));
        }
        var response = news.Adapt<NewsResponse>();
        if (news.LinkedProducts != null && news.LinkedProducts.Count != 0)
        {
            response = response with
            {
                LinkedProducts =
                    [.. news.LinkedProducts
                        .Select(
                            lp =>
                            {
                                var productName = lp.ProductVariant?.Product?.Name;
                                var variantName = lp.ProductVariant?.VariantName;
                                var colorName = lp.ProductVariantColor?.ColorName;
                                var productPrefix = productName != null ? $"{productName} - " : string.Empty;
                                var variantFullName = $"{productPrefix}{variantName}";
                                var displayName = lp.ProductVariantColor != null
                                    ? $"{variantFullName} ({colorName})"
                                    : variantFullName;
                                return new NewsProductResponse
                        {
                            Id = lp.Id.ToString(),
                            Name = displayName,
                            Price = "Liên hệ",
                            Img =
                                lp.ProductVariantColor?.CoverImageUrl ??
                                            lp.ProductVariant?.CoverImageUrl ??
                                            string.Empty,
                            ProductVariantId = lp.ProductVariantId,
                            ProductVariantColorId = lp.ProductVariantColorId,
                            VariantName = variantFullName,
                            ColorName = colorName
                        };
                            })]};
        }
        cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        return Result<NewsResponse>.Success(response);
    }
}
