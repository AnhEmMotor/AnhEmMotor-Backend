using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.InventoryOnHand;
using Domain.Primitives;
using MediatR;
using System.Linq;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportSummary
{
    public sealed class GetInventoryReportSummaryQueryHandler(IInventoryOnHandReadRepository readRepository) : IRequestHandler<GetInventoryReportSummaryQuery, Result<PagedResult<InventoryReportSummaryResponse>>>
    {
        public async Task<Result<PagedResult<InventoryReportSummaryResponse>>> Handle(
            GetInventoryReportSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var page = await readRepository.GetInventoryReportSummaryAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                cancellationToken)
                .ConfigureAwait(false);
            return new PagedResult<InventoryReportSummaryResponse>(
                page.Items,
                page.TotalCount,
                page.PageNumber,
                page.PageSize);
        }
    }
}
