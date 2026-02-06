using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public sealed class GetDailyRevenueQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetDailyRevenueQuery, Result<IEnumerable<DailyRevenueResponse>>>
{
    public async Task<Result<IEnumerable<DailyRevenueResponse>>> Handle(
        GetDailyRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetDailyRevenueAsync(request.Days, cancellationToken).ConfigureAwait(false);
        if(result == null)
        {
            return Result<IEnumerable<DailyRevenueResponse>>.Failure(Error.NotFound("Daily revenue not found"));
        }
        return Result<IEnumerable<DailyRevenueResponse>>.Success(result);
    }
}
