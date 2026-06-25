using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.DeleteManyPurchaseRequests;

public sealed record DeleteManyPurchaseRequestsCommand : IRequest<Result<int>>
{
    public List<int> Ids { get; init; } = [];
}
