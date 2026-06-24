using MediatR;
using Sieve.Models;

namespace Application.Features.PurchaseRequests.Queries.ExportPurchaseRequests;

public sealed record ExportPurchaseRequestsQuery : IRequest<byte[]>
{
    public SieveModel SieveModel { get; init; } = new();
}
