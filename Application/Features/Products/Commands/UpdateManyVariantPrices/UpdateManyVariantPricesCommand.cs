
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateManyVariantPrices;

public sealed record UpdateManyVariantPricesCommand : IRequest<Result<List<int>>>
{
    public List<int>? Ids { get; init; }
    public decimal Price { get; init; }
}

