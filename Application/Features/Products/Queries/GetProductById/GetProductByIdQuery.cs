using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public int Id { get; init; }
}
