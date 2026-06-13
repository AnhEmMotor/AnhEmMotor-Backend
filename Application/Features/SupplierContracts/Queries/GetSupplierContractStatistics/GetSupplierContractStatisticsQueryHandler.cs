using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierContract;
using Domain.Constants;
using MediatR;

namespace Application.Features.SupplierContracts.Queries.GetSupplierContractStatistics;

public class GetSupplierContractStatisticsQueryHandler(ISupplierContractReadRepository repository) : IRequestHandler<GetSupplierContractStatisticsQuery, Result<SupplierContractStatisticsResponse>>
{
    public async Task<Result<SupplierContractStatisticsResponse>> Handle(
        GetSupplierContractStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var total = await repository.CountAsync(cancellationToken).ConfigureAwait(false);
        var active = await repository.CountByStatusAsync(SupplierContractStatus.Active, cancellationToken)
            .ConfigureAwait(false);
        var pending = await repository.CountByStatusAsync(SupplierContractStatus.PendingApproval, cancellationToken)
            .ConfigureAwait(false);
        var expired = await repository.CountByStatusAsync(SupplierContractStatus.Expired, cancellationToken)
            .ConfigureAwait(false);
        var expiring = await repository.CountExpiringAsync(30, cancellationToken).ConfigureAwait(false);
        return new SupplierContractStatisticsResponse
        {
            TotalContracts = total,
            ActiveContracts = active,
            PendingApproval = pending,
            ExpiredContracts = expired,
            ExpiringContracts = expiring
        };
    }
}
