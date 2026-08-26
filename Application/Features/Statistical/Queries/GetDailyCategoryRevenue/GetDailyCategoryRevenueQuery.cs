using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyCategoryRevenue;

public record GetDailyCategoryRevenueQuery(
    int Days,
    DateTimeOffset? Start = null,
    DateTimeOffset? End = null) : IRequest<Result<IEnumerable<DailyCategoryRevenueResponse>>>;

public class GetDailyCategoryRevenueQueryHandler(IStatisticalReadRepository repo) : IRequestHandler<GetDailyCategoryRevenueQuery, Result<IEnumerable<DailyCategoryRevenueResponse>>>
{
    public async Task<Result<IEnumerable<DailyCategoryRevenueResponse>>> Handle(
        GetDailyCategoryRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var end = NormalizeToUtc(request.End ?? DateTimeOffset.UtcNow);
        var days = request.Days > 0 ? request.Days : 30;
        var start = NormalizeToUtc(request.Start ?? end.AddDays(-days));
        var result = await repo.GetDailyCategoryRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        return Result<IEnumerable<DailyCategoryRevenueResponse>>.Success(result);
    }

    private static DateTimeOffset NormalizeToUtc(DateTimeOffset value) =>
        value.TimeOfDay == TimeSpan.Zero
            ? new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, TimeSpan.Zero)
            : value.ToUniversalTime();
}
