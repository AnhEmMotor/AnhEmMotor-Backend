using Application.ApiContracts.Product.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.UpdateVariantPrice;

public sealed record UpdateVariantPriceCommand(int VariantId, decimal Price) : IRequest<Result<ProductVariantLiteResponse?>>;
