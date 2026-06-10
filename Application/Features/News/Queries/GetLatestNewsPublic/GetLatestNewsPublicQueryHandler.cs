using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.News;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Application.Features.News.Queries.GetLatestNewsPublic;

public sealed class GetLatestNewsPublicQueryHandler(INewsReadRepository repository) : IRequestHandler<GetLatestNewsPublicQuery, Result<List<NewsSummaryResponse>>>
{
    public async Task<Result<List<NewsSummaryResponse>>> Handle(
        GetLatestNewsPublicQuery request,
        CancellationToken cancellationToken)
    {
        // Enforce IsPublished == true strictly for store public api
        Expression<Func<Domain.Entities.News, bool>> filter = x => x.IsPublished;

        var sieveModel = new SieveModel
        {
            Page = 1,
            PageSize = 5,
            Sorts = "-PublishedDate"
        };

        var result = await repository.GetPagedAsync<NewsSummaryResponse>(
            sieveModel,
            filter: filter,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
            
        return result.Items?.ToList() ?? new List<NewsSummaryResponse>();
    }
}
