using Application.ApiContracts.Statistical.Responses;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetRecentTransactions;

public class GetRecentTransactionsQueryHandler(IStatisticalAnalyticsRepository analyticsRepository) : IRequestHandler<GetRecentTransactionsQuery, List<TransactionLogResponse>>
{
    public Task<List<TransactionLogResponse>> Handle(
        GetRecentTransactionsQuery request,
        CancellationToken cancellationToken) => analyticsRepository.GetRecentTransactionsAsync(
        request.Limit,
        cancellationToken);
}
