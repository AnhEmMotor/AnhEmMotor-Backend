using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportSummary
{
    public class GetInventoryReportSummaryQuery : IRequest<Result<PagedResult<InventoryReportSummaryResponse>>>
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; }
    }
}

