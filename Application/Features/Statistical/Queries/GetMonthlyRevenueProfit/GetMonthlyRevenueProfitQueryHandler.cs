using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed class GetMonthlyRevenueProfitQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetMonthlyRevenueProfitQuery, IEnumerable<MonthlyRevenueProfitResponse>>
{
    public Task<IEnumerable<MonthlyRevenueProfitResponse>> Handle(
        GetMonthlyRevenueProfitQuery request,
        CancellationToken cancellationToken)
    { return repository.GetMonthlyRevenueProfitAsync(request.Months, cancellationToken); }
}
