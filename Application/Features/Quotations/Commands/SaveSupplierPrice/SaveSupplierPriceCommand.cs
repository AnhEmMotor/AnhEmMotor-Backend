using Application.Common.Models;
using MediatR;

namespace Application.Features.Quotations.Commands.SaveSupplierPrice
{
    public sealed record SaveSupplierPriceCommand : IRequest<Result<bool>>
    {
        public int ProductVariantId { get; init; }
        public int? ProductVariantColorId { get; init; }
        public int SupplierId { get; init; }
        public int QuotePrice { get; init; }
        public string? Note { get; init; }
    }
}
