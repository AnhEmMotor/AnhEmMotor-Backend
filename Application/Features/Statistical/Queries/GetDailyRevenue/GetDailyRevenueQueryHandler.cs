using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetDailyRevenue;

public sealed class GetDailyRevenueQueryHandler(
    IStatisticalReadRepository repository) : IRequestHandler<GetDailyRevenueQuery, IEnumerable<DailyRevenueDto>>
{
    public Task<IEnumerable<DailyRevenueDto>> Handle(
        GetDailyRevenueQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetDailyRevenueAsync(
            request.Days,
            cancellationToken);
    }
}
