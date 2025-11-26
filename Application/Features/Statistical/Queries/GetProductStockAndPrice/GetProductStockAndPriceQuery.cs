using Application.Interfaces.Repositories.Statistical;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed record GetProductStockAndPriceQuery(int VariantId) : IRequest<ProductStockPriceDto?>;
