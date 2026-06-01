using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportSummary
{
    public sealed class GetInventoryReportSummaryQueryHandler : IRequestHandler<GetInventoryReportSummaryQuery, Result<PagedResult<InventoryReportSummaryResponse>>>
    {
        public Task<Result<PagedResult<InventoryReportSummaryResponse>>> Handle(
            GetInventoryReportSummaryQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
