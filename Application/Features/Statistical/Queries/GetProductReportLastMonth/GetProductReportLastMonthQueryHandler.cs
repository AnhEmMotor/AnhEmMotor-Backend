using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductReportLastMonth;

public sealed class GetProductReportLastMonthQueryHandler(IStatisticalReadRepository repository) : IRequestHandler<GetProductReportLastMonthQuery, Result<IEnumerable<ProductReportResponse>>>
{
    public async Task<Result<IEnumerable<ProductReportResponse>>> Handle(
        GetProductReportLastMonthQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.GetProductReportLastMonthAsync(cancellationToken).ConfigureAwait(false);
        return Result<IEnumerable<ProductReportResponse>>.Success(result);
    }
}
