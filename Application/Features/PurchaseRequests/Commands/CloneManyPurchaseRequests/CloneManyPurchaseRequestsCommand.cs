using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.CloneManyPurchaseRequests;

public sealed record CloneManyPurchaseRequestsCommand : IRequest<Result<int>>
{
    public List<int> Ids { get; init; } = [];
}
