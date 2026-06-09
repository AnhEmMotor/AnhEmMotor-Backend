using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.News.Queries.GetNewsListForStore;

public sealed record GetNewsListForStoreQuery : IRequest<Result<PagedResult<NewsSummaryResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
