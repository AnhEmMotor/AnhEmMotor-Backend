using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetMonthlyRevenueProfit;

public sealed class GetMonthlyRevenueProfitQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetMonthlyRevenueProfitQuery, Result<IEnumerable<MonthlyRevenueProfitResponse>>>
{
    public async Task<Result<IEnumerable<MonthlyRevenueProfitResponse>>> Handle(
        GetMonthlyRevenueProfitQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetMonthlyRevenueProfitAsync(request.Months, cancellationToken)
            .ConfigureAwait(false);
        return Result<IEnumerable<MonthlyRevenueProfitResponse>>.Success(result);
    }
}
