using Application.ApiContracts.PurchaseRequest.Responses;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.ProductQuotations.Queries.GetSupplierPricesForVariant
{
    public sealed record GetSupplierPricesForVariantQuery(int VariantId, int? ColorId) : IRequest<Result<List<PurchaseRequestQuotedPriceResponse>>>;
}
