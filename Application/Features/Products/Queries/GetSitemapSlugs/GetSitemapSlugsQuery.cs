using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetSitemapSlugs;

public sealed record GetSitemapSlugsQuery : IRequest<Result<SitemapSlugsResponse>>;
