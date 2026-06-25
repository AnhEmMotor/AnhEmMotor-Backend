using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenueDetail;

public sealed class GetDailyRevenueDetailQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetDailyRevenueDetailQuery, Result<IEnumerable<DailyRevenueDetailResponse>>>
{
    public async Task<Result<IEnumerable<DailyRevenueDetailResponse>>> Handle(
        GetDailyRevenueDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(request.ReportDay, out var reportDay))
        {
            return Result<IEnumerable<DailyRevenueDetailResponse>>.Failure(Error.Validation("Invalid date format"));
        }
        var result = await repository
            .GetDailyRevenueDetailAsync(reportDay, request.Days, cancellationToken)
            .ConfigureAwait(false);
        if (result is null || !result.Any())
        {
            return Result<IEnumerable<DailyRevenueDetailResponse>>.Success([]);
        }
        return Result<IEnumerable<DailyRevenueDetailResponse>>.Success(result);
    }
}
