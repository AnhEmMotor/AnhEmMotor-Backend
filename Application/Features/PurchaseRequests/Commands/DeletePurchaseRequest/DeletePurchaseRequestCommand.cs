using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest
{
    public sealed record DeletePurchaseRequestCommand(int Id, bool HasApproveRejectPermission) : IRequest<Result>;
}
