using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.RestorePurchaseRequest;

public sealed record RestorePurchaseRequestCommand : IRequest<Result<int>>
{
    public int Id { get; init; }
}
