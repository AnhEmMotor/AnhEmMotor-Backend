using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.DeleteSupplierPrice
{
    public sealed record DeleteSupplierPriceCommand(int ProductVariantId, int? ProductVariantColorId, int SupplierId) : IRequest<Result<bool>>;
}
