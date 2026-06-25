using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Queries.GetNewsById;

public sealed record GetNewsByIdQuery : IRequest<Result<NewsResponse>>
{
    public int Id { get; init; }
}
