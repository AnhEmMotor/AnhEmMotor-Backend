using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById
{
    public sealed record GetPurchaseRequestByIdQuery(int Id) : IRequest<Result<PurchaseRequestDetailResponse?>>;
}
