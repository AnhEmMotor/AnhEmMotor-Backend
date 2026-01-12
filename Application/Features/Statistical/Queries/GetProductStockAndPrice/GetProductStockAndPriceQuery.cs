using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed record GetProductStockAndPriceQuery : IRequest<Result<ProductStockPriceResponse?>>
{
    public int VariantId { get; init; }
}
