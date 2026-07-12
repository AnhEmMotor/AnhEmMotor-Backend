using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyCategoryRevenue;

public record GetDailyCategoryRevenueQuery(int Days) : IRequest<Result<IEnumerable<DailyCategoryRevenueResponse>>>;

public class GetDailyCategoryRevenueQueryHandler(IStatisticalReadRepository repo) : IRequestHandler<GetDailyCategoryRevenueQuery, Result<IEnumerable<DailyCategoryRevenueResponse>>>
{
    public async Task<Result<IEnumerable<DailyCategoryRevenueResponse>>> Handle(
        GetDailyCategoryRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var _now = DateTimeOffset.UtcNow;
        var end = _now;
        var days = request.Days > 0 ? request.Days : 30;
        var start = _now.AddDays(-days);
        var result = await repo.GetDailyCategoryRevenueAsync(start, end, cancellationToken).ConfigureAwait(false);
        return Result<IEnumerable<DailyCategoryRevenueResponse>>.Success(result);
    }
}
