using Application.ApiContracts.Staticals;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed class GetOrderStatusCountsQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetOrderStatusCountsQuery, IEnumerable<OrderStatusCountResponse>>
{
    public Task<IEnumerable<OrderStatusCountResponse>> Handle(
        GetOrderStatusCountsQuery request,
        CancellationToken cancellationToken)
    { return repository.GetOrderStatusCountsAsync(cancellationToken); }
}
