using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed record RestoreManyProductsCommand : IRequest<Result<List<ProductDetailForManagerResponse>?>>
{
    public List<int>? Ids { get; init; }
}

