using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Commands.SendPurchaseOrder
{
    public sealed record SendPurchaseOrderCommand(int Id) : IRequest<Result>;
}
