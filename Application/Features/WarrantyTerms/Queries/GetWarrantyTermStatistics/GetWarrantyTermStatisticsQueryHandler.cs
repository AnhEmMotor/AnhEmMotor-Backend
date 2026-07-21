using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Constants;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermStatistics;

public class GetWarrantyTermStatisticsQueryHandler(
    IWarrantyTermReadRepository readRepository) : IRequestHandler<GetWarrantyTermStatisticsQuery, Result<WarrantyTermStatisticsResponse>>
{
    public async Task<Result<WarrantyTermStatisticsResponse>> Handle(
        GetWarrantyTermStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var stats = await readRepository.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);
        return stats;
    }
}
