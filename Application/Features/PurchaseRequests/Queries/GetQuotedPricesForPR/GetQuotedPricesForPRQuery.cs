using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.PurchaseRequests.Queries.GetQuotedPricesForPR
{
    public sealed record GetQuotedPricesForPRQuery(int Id) : IRequest<Result<List<PurchaseRequestQuotedPriceResponse>>>;
}
