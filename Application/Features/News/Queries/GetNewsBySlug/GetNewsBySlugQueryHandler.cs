using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.News.Queries.GetNewsBySlug;

public sealed class GetNewsBySlugQueryHandler(INewsReadRepository repository, IMemoryCache cache) : IRequestHandler<GetNewsBySlugQuery, Result<NewsResponse>>
{
    public async Task<Result<NewsResponse>> Handle(GetNewsBySlugQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"News_{request.Slug}";
        if (cache.TryGetValue(cacheKey, out NewsResponse? cachedResponse))
        {
            return Result<NewsResponse>.Success(cachedResponse!);
        }

        var news = await repository.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (news == null)
        {
            return Result<NewsResponse>.Failure(new Error("News.NotFound", "The requested news was not found."));
        }

        var response = news.Adapt<NewsResponse>();
        cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        return Result<NewsResponse>.Success(response);
    }
}
