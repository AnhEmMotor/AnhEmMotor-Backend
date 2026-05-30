using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.SendPurchaseRequest
{
    public sealed record SendPurchaseRequestCommand(int Id) : IRequest<Result>;
}
