
using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProductStatus;

public sealed record UpdateProductStatusCommand : IRequest<Result<ProductDetailForManagerResponse?>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }
}
