using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed class GetMonthlyRevenueProfitQueryHandler(
    IStatisticalReadRepository repository) : IRequestHandler<GetMonthlyRevenueProfitQuery, IEnumerable<MonthlyRevenueProfitDto>>
{
    public Task<IEnumerable<MonthlyRevenueProfitDto>> Handle(
        GetMonthlyRevenueProfitQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetMonthlyRevenueProfitAsync(
            request.Months,
            cancellationToken);
    }
}
