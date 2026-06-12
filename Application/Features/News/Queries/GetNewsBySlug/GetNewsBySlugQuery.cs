using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Queries.GetNewsBySlug;

public sealed record GetNewsBySlugQuery : IRequest<Result<NewsForStoreResponse>>
{
    public string Slug { get; init; } = default!;
}
