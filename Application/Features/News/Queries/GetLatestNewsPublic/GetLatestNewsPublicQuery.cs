using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Queries.GetLatestNewsPublic;

public sealed record GetLatestNewsPublicQuery : IRequest<Result<List<NewsSummaryResponse>>>
{
}
