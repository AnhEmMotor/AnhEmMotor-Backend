using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryReports.Queries.ExportInventoryReport
{
    public class ExportInventoryReportQuery : IRequest<Result<FileStreamResult>>
    {
        public string? SearchTerm { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }
    }
}
