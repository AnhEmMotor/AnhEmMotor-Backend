
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public int Id { get; init; }
}

