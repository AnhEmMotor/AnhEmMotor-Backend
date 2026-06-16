namespace Application.Features.Statistical.Queries.GetDailyRevenueDetail;

using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

public sealed class GetDailyRevenueDetailQuery : IRequest<Result<IEnumerable<DailyRevenueDetailResponse>>>
{
    public required string ReportDay { get; init; }
    public int Days { get; init; } = 7;
}
