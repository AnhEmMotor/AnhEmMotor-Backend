using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest
{
    public sealed record ApproveRejectPurchaseRequestCommand(int Id, string Status) : IRequest<Result>;
}
