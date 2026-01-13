using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed class GetOrderStatusCountsQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetOrderStatusCountsQuery, Result<IEnumerable<OrderStatusCountResponse>>>
{
    public async Task<Result<IEnumerable<OrderStatusCountResponse>>> Handle(
        GetOrderStatusCountsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetOrderStatusCountsAsync(cancellationToken).ConfigureAwait(false);
        return Result<IEnumerable<OrderStatusCountResponse>>.Success(result);
    }
}
