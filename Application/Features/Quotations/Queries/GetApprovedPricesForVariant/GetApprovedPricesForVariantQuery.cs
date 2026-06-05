using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Quotations.Queries.GetApprovedPricesForVariant
{
    public sealed record GetApprovedPricesForVariantQuery(int VariantId, int? ColorId) : IRequest<Result<List<PurchaseRequestQuotedPriceResponse>>>;
}
