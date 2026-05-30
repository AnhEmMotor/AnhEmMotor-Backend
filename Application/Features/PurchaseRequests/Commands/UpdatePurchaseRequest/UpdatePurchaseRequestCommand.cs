using Application.ApiContracts.PurchaseRequest.Requests;
using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest
{
    public sealed record UpdatePurchaseRequestCommand : IRequest<Result<PurchaseRequestDetailResponse?>>
    {
        public int Id { get; init; }

        public string? Note { get; init; }

        public List<UpdatePurchaseRequestItemRequest> Items { get; init; } = [];
    }
}
