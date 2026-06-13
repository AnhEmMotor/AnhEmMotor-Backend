using Application.Common.Models;
using Application.Interfaces.Repositories.SalesContract;
using Domain.Constants;
using MediatR;

namespace Application.Features.SalesContracts.Queries.GetSalesContractStatistics;

public class GetSalesContractStatisticsQueryHandler(
    ISalesContractReadRepository readRepo) : IRequestHandler<GetSalesContractStatisticsQuery, Result<SalesContractStatisticsResponse>>
{
    public async Task<Result<SalesContractStatisticsResponse>> Handle(
        GetSalesContractStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var draftCount = await readRepo.CountByStatusAsync(SalesContractStatus.Draft, cancellationToken).ConfigureAwait(false);
        var signedCount = await readRepo.CountByStatusAsync(SalesContractStatus.Signed, cancellationToken).ConfigureAwait(false);
        var total = await readRepo.CountAsync(cancellationToken).ConfigureAwait(false);
        var fulfilledCount = total - draftCount - signedCount;
        var overdueCount = await readRepo.CountOverdueAsync(cancellationToken).ConfigureAwait(false);

        return Result<SalesContractStatisticsResponse>.Success(new SalesContractStatisticsResponse
        {
            DraftCount = draftCount,
            SignedCount = signedCount,
            FulfilledCount = Math.Max(0, fulfilledCount),
            OverdueCount = overdueCount,
        });
    }
}
