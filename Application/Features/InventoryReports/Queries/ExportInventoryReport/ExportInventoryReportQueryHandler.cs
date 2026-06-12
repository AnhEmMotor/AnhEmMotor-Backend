using Application.Common.Models;
using MediatR;
using System;

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
