using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.News.Queries.GetNewsList;

public sealed record GetNewsListQuery : IRequest<Result<PagedResult<NewsResponse>>>
{
    public SieveModel? SieveModel { get; init; }
}
