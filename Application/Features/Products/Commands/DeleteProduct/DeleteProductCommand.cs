
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand : IRequest<Result>
{
    public int Id { get; init; }
}

