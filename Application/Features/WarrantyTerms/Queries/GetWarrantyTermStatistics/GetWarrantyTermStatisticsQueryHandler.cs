using Application.ApiContracts.WarrantyTerms.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermStatistics;

public class GetWarrantyTermStatisticsQueryHandler(IWarrantyTermReadRepository readRepository) : IRequestHandler<GetWarrantyTermStatisticsQuery, Result<WarrantyTermStatisticsResponse>>
{
    public async Task<Result<WarrantyTermStatisticsResponse>> Handle(
        GetWarrantyTermStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var terms = await readRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var totalCount = terms.Count;
        var activeCount = terms.Count(x => x.Status == "Active");
        var expiredCount = totalCount - activeCount;
        return Result<WarrantyTermStatisticsResponse>.Success(
            new WarrantyTermStatisticsResponse
            {
                TotalCount = totalCount,
                ActiveCount = activeCount,
                ExpiredCount = expiredCount
            });
    }
}
