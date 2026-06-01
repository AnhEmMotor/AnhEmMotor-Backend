using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryReports.Queries.ExportInventoryReport
{
    public sealed class ExportInventoryReportQueryHandler : IRequestHandler<ExportInventoryReportQuery, Result<FileStreamResult>>
    {
        public Task<Result<FileStreamResult>> Handle(
            ExportInventoryReportQuery request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
