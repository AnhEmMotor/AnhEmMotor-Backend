using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseOrders.Commands.DeletePurchaseOrder
{
    public sealed record DeletePurchaseOrderCommand(int Id) : IRequest<Result>;
}
