using Application.Common.Models;
using MediatR;

namespace Application.Features.ProductQuotations.Commands.DeleteSupplierPrice
{
    public sealed record DeleteSupplierPriceCommand(int ProductVariantId, int? ProductVariantColorId, int SupplierId) : IRequest<Result<bool>>;
}
