using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.GetInventoryReportDetail
{
    public sealed class GetInventoryReportDetailQueryHandler : IRequestHandler<GetInventoryReportDetailQuery, Result<InventoryReportDetailResponse>>
    {
        public Task<Result<InventoryReportDetailResponse>> Handle(
            GetInventoryReportDetailQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
