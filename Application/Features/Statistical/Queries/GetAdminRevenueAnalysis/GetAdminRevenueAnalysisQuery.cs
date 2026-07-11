using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetAdminRevenueAnalysis;

public sealed record GetAdminRevenueAnalysisQuery : IRequest<Result<AdminRevenueAnalysisResponse>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
