using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Queries.GetBrandStatistics;

public sealed record GetBrandStatisticsQuery : IRequest<Result<BrandStatisticsResponse>>;
