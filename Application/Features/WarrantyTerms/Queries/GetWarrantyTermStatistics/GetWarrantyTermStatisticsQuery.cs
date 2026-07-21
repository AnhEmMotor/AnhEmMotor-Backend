using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermStatistics;

public sealed record GetWarrantyTermStatisticsQuery : IRequest<Result<WarrantyTermStatisticsResponse>>;
