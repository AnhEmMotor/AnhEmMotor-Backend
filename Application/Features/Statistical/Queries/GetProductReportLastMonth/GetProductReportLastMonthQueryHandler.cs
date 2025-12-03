using Application.ApiContracts.Staticals;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed class GetProductReportLastMonthQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetProductReportLastMonthQuery, IEnumerable<ProductReportResponse>>
{
    public Task<IEnumerable<ProductReportResponse>> Handle(
        GetProductReportLastMonthQuery request,
        CancellationToken cancellationToken)
    { return repository.GetProductReportLastMonthAsync(cancellationToken); }
}
