using Application.ApiContracts.PurchaseOrder.Requests;
using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder
{
    public sealed record UpdatePurchaseOrderCommand : IRequest<Result<PurchaseOrderDetailResponse?>>
    {
        public int Id { get; init; }

        public int? PurchaseRequestId { get; init; }

        public int SupplierId { get; init; }

        public string? Note { get; init; }

        public List<UpdatePurchaseOrderItemRequest> Items { get; init; } = [];
    }
}
