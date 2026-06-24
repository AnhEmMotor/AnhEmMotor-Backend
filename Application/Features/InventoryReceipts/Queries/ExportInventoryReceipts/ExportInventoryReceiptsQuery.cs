using MediatR;
using Sieve.Models;

namespace Application.Features.InventoryReceipts.Queries.ExportInventoryReceipts;

public sealed record ExportInventoryReceiptsQuery : IRequest<byte[]>
{
    public SieveModel SieveModel { get; init; } = new();
}
