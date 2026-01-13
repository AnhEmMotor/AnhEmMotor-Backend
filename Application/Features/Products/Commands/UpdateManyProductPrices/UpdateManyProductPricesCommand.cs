
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed record UpdateManyProductPricesCommand : IRequest<Result<List<int>?>>
{
    public List<int>? Ids { get; init; }
    public decimal Price { get; init; }
}
