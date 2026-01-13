using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed record UpdateVariantPriceCommand: IRequest<Result<ProductVariantLiteResponse?>>
{
    public int VariantId { get; init; }
    public decimal Price { get; init; }
}

