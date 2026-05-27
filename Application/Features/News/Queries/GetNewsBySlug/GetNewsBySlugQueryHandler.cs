using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Mapster;
using MediatR;

namespace Application.Features.News.Queries.GetNewsBySlug;

public sealed class GetNewsBySlugQueryHandler(INewsReadRepository repository) : IRequestHandler<GetNewsBySlugQuery, Result<NewsResponse>>
{
    public async Task<Result<NewsResponse>> Handle(GetNewsBySlugQuery request, CancellationToken cancellationToken)
    {
        var news = await repository.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (news == null)
        {
            return Result<NewsResponse>.Failure(new Error("News.NotFound", "The requested news was not found."));
        }
        return news.Adapt<NewsResponse>();
    }
}
