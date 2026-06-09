using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Domain.Constants;
using Domain.Primitives;
using MediatR;

namespace Application.Features.News.Queries.GetNewsList;

public sealed class GetNewsListQueryHandler(INewsReadRepository repository) : IRequestHandler<GetNewsListQuery, Result<PagedResult<NewsSummaryResponse>>>
{
    public async Task<Result<PagedResult<NewsSummaryResponse>>> Handle(
        GetNewsListQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync<NewsSummaryResponse>(
            request.SieveModel!,
            DataFetchMode.All,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
