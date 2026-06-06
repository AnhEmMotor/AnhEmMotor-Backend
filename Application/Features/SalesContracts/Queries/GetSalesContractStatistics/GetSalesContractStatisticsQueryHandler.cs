using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractStatistics;

public sealed class GetSalesContractStatisticsQueryHandler(
    ISalesContractReadRepository readRepo) : IRequestHandler<GetSalesContractStatisticsQuery, Result<SalesContractStatisticsResponse>>
{
    public async Task<Result<SalesContractStatisticsResponse>> Handle(
        GetSalesContractStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var draftCount = await readRepo.CountByStatusAsync("Draft", cancellationToken);
        var signedCount = await readRepo.CountByStatusAsync("Signed", cancellationToken);
        var total = await readRepo.CountAsync(cancellationToken);
        var fulfilledCount = total - draftCount - signedCount;
        var overdueCount = await readRepo.CountOverdueAsync(cancellationToken);

        return Result<SalesContractStatisticsResponse>.Success(new SalesContractStatisticsResponse
        {
            DraftCount = draftCount,
            SignedCount = signedCount,
            FulfilledCount = Math.Max(0, fulfilledCount),
            OverdueCount = overdueCount,
        });
    }
}
