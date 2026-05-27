using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Domain.Primitives;
using MediatR;

namespace Application.Features.News.Queries.GetNewsList;

public sealed class GetNewsListQueryHandler(INewsReadRepository repository) : IRequestHandler<GetNewsListQuery, Result<PagedResult<NewsResponse>>>
{
    public async Task<Result<PagedResult<NewsResponse>>> Handle(
        GetNewsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<NewsResponse>(
            request.SieveModel!,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
