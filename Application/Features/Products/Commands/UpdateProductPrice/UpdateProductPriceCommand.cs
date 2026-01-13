using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductPrice;

public sealed record UpdateProductPriceCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public int Id { get; init; }
    public decimal Price { get; init; }
}


