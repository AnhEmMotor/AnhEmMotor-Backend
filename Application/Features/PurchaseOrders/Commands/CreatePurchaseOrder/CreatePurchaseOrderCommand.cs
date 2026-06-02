using Application.ApiContracts.PurchaseOrder.Requests;
using Application.ApiContracts.PurchaseOrder.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder
{
    public sealed record CreatePurchaseOrderCommand : IRequest<Result<List<PurchaseOrderDetailResponse>>>
    {
        public int? PurchaseRequestId { get; init; }

        public string? Note { get; init; }

        public List<CreatePurchaseOrderItemRequest> Items { get; init; } = [];
    }
}
