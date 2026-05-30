using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportDetail
{
    public class GetInventoryReportDetailQuery : IRequest<Result<InventoryReportDetailResponse>>
    {
        public int VariantId { get; set; }
        public int? ColorId { get; set; }
    }
}
