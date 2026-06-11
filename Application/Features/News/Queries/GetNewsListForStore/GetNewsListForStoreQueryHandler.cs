using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using Domain.Primitives;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.News.Queries.GetNewsListForStore;

public sealed class GetNewsListForStoreQueryHandler(INewsReadRepository repository) : IRequestHandler<GetNewsListForStoreQuery, Result<PagedResult<NewsSummaryResponse>>>
{
    public async Task<Result<PagedResult<NewsSummaryResponse>>> Handle(
        GetNewsListForStoreQuery request,
        CancellationToken cancellationToken)
    {
        Expression<Func<Domain.Entities.News, bool>> filter = x => x.IsPublished;
        var result = await repository.GetPagedAsync<NewsSummaryResponse>(
            request.SieveModel!,
            filter: filter,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result;
    }
}
