using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetOrderStatusCounts;

public sealed class GetOrderStatusCountsQueryHandler(
    IStatisticalReadRepository repository) : IRequestHandler<GetOrderStatusCountsQuery, IEnumerable<OrderStatusCountDto>>
{
    public Task<IEnumerable<OrderStatusCountDto>> Handle(
        GetOrderStatusCountsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetOrderStatusCountsAsync(cancellationToken);
    }
}
