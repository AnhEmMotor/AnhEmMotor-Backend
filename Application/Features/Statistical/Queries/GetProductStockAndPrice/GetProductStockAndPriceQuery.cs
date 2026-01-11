using Application.ApiContracts.Statistical.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Statistical.Queries.GetProductStockAndPrice;

public sealed record GetProductStockAndPriceQuery(int VariantId) : IRequest<Result<ProductStockPriceResponse?>>;
