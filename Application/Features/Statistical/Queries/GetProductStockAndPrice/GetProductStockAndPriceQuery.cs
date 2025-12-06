using Application.ApiContracts.Statistical.Responses;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed record GetProductStockAndPriceQuery(int VariantId) : IRequest<ProductStockPriceResponse?>;
