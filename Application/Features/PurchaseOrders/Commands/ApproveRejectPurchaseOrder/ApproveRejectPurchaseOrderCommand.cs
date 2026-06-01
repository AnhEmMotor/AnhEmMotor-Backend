using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Commands.ApproveRejectPurchaseOrder
{
    public sealed record ApproveRejectPurchaseOrderCommand(int Id, string Status) : IRequest<Result>;
}
