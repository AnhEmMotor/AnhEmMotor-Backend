using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Supplier;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetSupplierStatistics;

public sealed class GetSupplierStatisticsQueryHandler(
    ISupplierReadRepository repository) : IRequestHandler<GetSupplierStatisticsQuery, Result<SupplierStatisticsResponse>>
{
    public async Task<Result<SupplierStatisticsResponse>> Handle(
        GetSupplierStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var statistics = await repository.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);
        return Result<SupplierStatisticsResponse>.Success(statistics);
    }
}