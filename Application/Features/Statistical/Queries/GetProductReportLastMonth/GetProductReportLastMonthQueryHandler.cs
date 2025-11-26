using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed class GetProductReportLastMonthQueryHandler(
    IStatisticalReadRepository repository) : IRequestHandler<GetProductReportLastMonthQuery, IEnumerable<ProductReportDto>>
{
    public Task<IEnumerable<ProductReportDto>> Handle(
        GetProductReportLastMonthQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetProductReportLastMonthAsync(cancellationToken);
    }
}
