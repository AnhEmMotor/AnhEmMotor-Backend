using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.News;
using Domain.Primitives;
using MediatR;
using NewsEntity = Domain.Entities.News;

namespace Application.Features.News.Queries.GetNewsList;

public sealed class GetNewsListQueryHandler(INewsReadRepository repository, ISievePaginator paginator) : IRequestHandler<GetNewsListQuery, Result<PagedResult<NewsResponse>>>
{
    public async Task<Result<PagedResult<NewsResponse>>> Handle(
        GetNewsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        query = query.Where(n => n.IsPublished);
        var pagedResult = await paginator.ApplyAsync<NewsEntity, NewsResponse>(
            query,
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return pagedResult;
    }
}
