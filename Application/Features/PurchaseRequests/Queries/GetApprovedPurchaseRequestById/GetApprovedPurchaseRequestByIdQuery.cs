using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetApprovedPurchaseRequestById
{
    public sealed record GetApprovedPurchaseRequestByIdQuery(int Id, int? ExcludePurchaseOrderId = null) : IRequest<Result<ApprovedPurchaseRequestDetailResponse?>>;
}
