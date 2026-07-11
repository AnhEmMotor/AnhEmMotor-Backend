using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public class GetDailyRevenueQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetDailyRevenueQuery, Result<IEnumerable<DailyRevenueResponse>>>
{
    public async Task<Result<IEnumerable<DailyRevenueResponse>>> Handle(
        GetDailyRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var _now = DateTimeOffset.UtcNow;
        var end = _now;
        var days = request.Days > 0 ? request.Days : 30;
        var start = _now.AddDays(-days);

        var result = await repository.GetDailyRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        if (result == null)
        {
            return Result<IEnumerable<DailyRevenueResponse>>.Failure(Error.NotFound("Daily revenue not found"));
        }
        return Result<IEnumerable<DailyRevenueResponse>>.Success(result);
    }
}
