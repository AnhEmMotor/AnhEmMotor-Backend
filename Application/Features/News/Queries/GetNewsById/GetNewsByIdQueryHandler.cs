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
        if (news.LinkedProducts != null && news.LinkedProducts.Any())
        {
            response = response with
            {
                LinkedProducts = news.LinkedProducts.Select(lp => new NewsProductResponse
                {
                    Id = lp.Id.ToString(),
                    Name = (lp.ProductVariant?.Product?.Name != null ? lp.ProductVariant.Product.Name + " - " : "") + 
                           lp.ProductVariant?.VariantName + 
                           (lp.ProductVariantColor != null ? $" ({lp.ProductVariantColor.ColorName})" : ""),
                    Price = "Liên hệ",
                    Img = lp.ProductVariantColor?.CoverImageUrl ?? lp.ProductVariant?.CoverImageUrl ?? "",
                    ProductVariantId = lp.ProductVariantId,
                    ProductVariantColorId = lp.ProductVariantColorId,
                    VariantName = (lp.ProductVariant?.Product?.Name != null ? lp.ProductVariant.Product.Name + " - " : "") + lp.ProductVariant?.VariantName,
                    ColorName = lp.ProductVariantColor?.ColorName
                }).ToList()
            };
        }

        cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        return Result<NewsResponse>.Success(response);
    }
}
