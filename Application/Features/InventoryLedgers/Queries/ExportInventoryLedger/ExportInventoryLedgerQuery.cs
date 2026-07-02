using Application.Common.Models;
using MediatR;

namespace Application.Features.InventoryLedgers.Queries.ExportInventoryLedger
{
    public class ExportInventoryLedgerQuery : IRequest<Result<FileStreamResult>>
    {
        public string? SearchTerm { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Type { get; set; }
    }
}
