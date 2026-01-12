
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed record DeleteManyProductsCommand : IRequest<Result>
{
    public List<int>? Ids { get; init; }
}
