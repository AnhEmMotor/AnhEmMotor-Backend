using Application.ApiContracts.PurchaseRequest.Requests;
using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest
{
    public sealed record CreatePurchaseRequestCommand : IRequest<Result<PurchaseRequestDetailResponse?>>
    {
        public string? Note { get; init; }

        public List<CreatePurchaseRequestItemRequest> Items { get; init; } = [];
    }
}
