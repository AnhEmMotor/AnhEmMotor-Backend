using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Queries.GetNewsBySlug;

public sealed record GetNewsBySlugQuery : IRequest<Result<NewsResponse>>
{
    public string Slug { get; init; } = string.Empty;
}
