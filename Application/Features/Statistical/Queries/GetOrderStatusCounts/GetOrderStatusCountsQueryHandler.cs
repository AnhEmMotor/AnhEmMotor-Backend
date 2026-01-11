using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed class GetOrderStatusCountsQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetOrderStatusCountsQuery, Result<IEnumerable<OrderStatusCountResponse>>>
{
    public Task<Result<IEnumerable<OrderStatusCountResponse>>> Handle(
        GetOrderStatusCountsQuery request,
        CancellationToken cancellationToken)
    { return repository.GetOrderStatusCountsAsync(cancellationToken); }
}
