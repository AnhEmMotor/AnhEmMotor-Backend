using Application.ApiContracts.News.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Queries.GetRelatedNewsPublic;

public sealed record GetRelatedNewsPublicQuery(string Slug) : IRequest<Result<List<NewsSummaryResponse>>>;
