using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.RestoreManyPurchaseRequests;

public sealed record RestoreManyPurchaseRequestsCommand : IRequest<Result<int>>
{
    public List<int> Ids { get; init; } = [];
}
