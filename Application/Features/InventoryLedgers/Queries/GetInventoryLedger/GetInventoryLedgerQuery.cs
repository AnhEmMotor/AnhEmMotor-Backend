using Application.ApiContracts.InventoryLedger.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.InventoryLedgers.Queries.GetInventoryLedger
{
    public class GetInventoryLedgerQuery : IRequest<Result<List<InventoryLedgerResponse>>>
    {
        public string? SearchQuery { get; set; }

        public string? Type { get; set; } = "ALL";

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }
    }
}
