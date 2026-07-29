using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetRecentTransactions;

public class GetRecentTransactionsQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetRecentTransactionsQuery, Result<List<TransactionLogResponse>>>
{
    public async Task<Result<List<TransactionLogResponse>>> Handle(
        GetRecentTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await analyticsRepository.GetRecentTransactionsAsync(
            request.Limit,
            cancellationToken);
        return result;
    }
}
